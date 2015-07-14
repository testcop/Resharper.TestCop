// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.TextControl;
using JetBrains.TextControl.DataConstants;
using JetBrains.UI.PopupMenu;
using JetBrains.Util;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;
using TestCop.Plugin.Helper.Mapper;
using DataConstants = JetBrains.TextControl.DataContext.DataConstants;
#if !R7
using JetBrains.ReSharper.Psi.Modules;
#endif

namespace TestCop.Plugin
{
    [ActionHandler("TestCop.JumpToTest")]
    public class JumpToTestFileAction : IActionHandler
    {
        private readonly Action<JetPopupMenu, JetPopupMenu.ShowWhen> _menuDisplayer = (menu, showWhen) => menu.Show(showWhen);

        readonly Func<IClrDeclaredElement, IClrDeclaredElement, bool> _declElementMatcher =
                    (element, declaredElement) => element.ToString() == declaredElement.ToString();

        public JumpToTestFileAction(Action<JetPopupMenu, JetPopupMenu.ShowWhen> overrideMenuDisplay): this()
        {
            _menuDisplayer = overrideMenuDisplay;
        }

        private static bool _alreadyMappedKeyboardHack = false;

        public JumpToTestFileAction()
        {
            if (!_alreadyMappedKeyboardHack)
            {
                _alreadyMappedKeyboardHack = true;
                ResharperHelper.AppendLineToOutputWindow(Assembly.GetExecutingAssembly().GetName().ToString());
                ResharperHelper.ForceKeyboardBindings();
            }
        }

        bool IActionHandler.Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            // fetch focused text editor control
            ITextControl textControl = context.GetData(DataConstants.TEXT_CONTROL);
            
            // enable this action if we are in text editor, disable otherwise            
            return textControl != null;
        }

   
     
        void IActionHandler.Execute(IDataContext context, DelegateExecute nextExecute)
        {            
            ITextControl textControl = context.GetData(DataConstants.TEXT_CONTROL);
            if (textControl == null)
            {
                MessageBox.ShowError("Text control unavailable.");                
                return;
            }
            ISolution solution = context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.SOLUTION);
            if (solution == null){return;}

            IClrTypeName clrTypeClassName = ResharperHelper.GetClassNameAppropriateToLocation(solution, textControl);
            if (clrTypeClassName == null) return;

            var currentProject = context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.Project);
            var targetProjects = currentProject.GetAssociatedProjects(clrTypeClassName.GetNamespaceName());     
            if(targetProjects.IsEmpty())
            {
                ResharperHelper.AppendLineToOutputWindow("Unable to locate associated assembly - check project namespaces and testcop Regex");
                //ProjectMappingHelper.GetProjectMappingHeper().DumpDebug(solution);
                return;
            }
                       
            var settings = solution.GetPsiServices().SettingsStore
                .BindToContextTransient(ContextRange.Smart(textControl.ToDataContext()))                
                .GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);
                                                
            var classNamesToFind = new List<string>();

            var baseFileName = ResharperHelper.GetBaseFileName(context, solution);
            bool isTestFile = baseFileName.EndsWith(settings.TestClassSuffixes());
           
            var elementsFoundInTarget = new List<IClrDeclaredElement>();
            var elementsFoundInSolution = new List<IClrDeclaredElement>();

            foreach (var testClassSuffix in settings.GetAppropriateTestClassSuffixes(baseFileName))
            {
                //change flip to create list of possible names..
                var classNameFromFileName = ResharperHelper.UsingFileNameGetClassName(baseFileName)
                    .Flip(isTestFile, testClassSuffix);                

                if (clrTypeClassName != null)
                {
                    string className = clrTypeClassName.ShortName.Flip(isTestFile, testClassSuffix);
                    elementsFoundInTarget.AddRangeIfMissing(
                        ResharperHelper.FindClass(solution, className, targetProjects), _declElementMatcher);

                    classNamesToFind.Add(className);
                    elementsFoundInSolution.AddRangeIfMissing(
                        ResharperHelper.FindClass(solution, className),_declElementMatcher);
                }

                classNamesToFind.Add(classNameFromFileName);
                elementsFoundInTarget.AddRangeIfMissing(
                    ResharperHelper.FindClass(solution, classNameFromFileName, targetProjects), _declElementMatcher);
                elementsFoundInSolution.AddRangeIfMissing(
                    ResharperHelper.FindClass(solution, classNameFromFileName),_declElementMatcher);

                if (!isTestFile)
                {
                    var references = FindReferencesWithinAssociatedAssembly(context, solution, textControl,
                        clrTypeClassName, testClassSuffix);
                    elementsFoundInTarget.AddRangeIfMissing(references, _declElementMatcher);
                }             
            }

            JumpToTestMenuHelper.PromptToOpenOrCreateClassFiles(_menuDisplayer, textControl.Lifetime, context,
                solution
                , currentProject, clrTypeClassName, targetProjects
                , elementsFoundInTarget, elementsFoundInSolution);
        }
        
        private TestFileAnalysisSettings Settings { 
            get
            {
                var settingsStore = Shell.Instance.GetComponent<ISettingsStore>();
                var contextBoundSettingsStore = settingsStore.BindToContextTransient(ContextRange.ApplicationWide);
                var mySettings = contextBoundSettingsStore.GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);                
                return mySettings; 
            }

        }

        private IList<IClrDeclaredElement> FindReferencesWithinAssociatedAssembly(IDataContext context, ISolution solution, ITextControl textControl, IClrTypeName clrTypeClassName, string testClassSuffix)
        {
            if (clrTypeClassName == null)
            {
                ResharperHelper.AppendLineToOutputWindow("FindReferencesWithinAssociatedAssembly() - clrTypeClassName was null");
                return new List<IClrDeclaredElement>();
            }

            IPsiServices services = solution.GetPsiServices();
            IProject currentProject = context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.Project);

            var targetProjects = currentProject.GetAssociatedProjects(clrTypeClassName.GetNamespaceName() );
            ISearchDomain searchDomain;

            if (Settings.FindAnyUsageInTestAssembly)
            {
                searchDomain = PsiShared.GetComponent<SearchDomainFactory>().CreateSearchDomain(                
                targetProjects.SelectMany(proj=>proj.Project.GetAllProjectFiles().Select(p => p.GetPsiModule())) );
            }
            else
            {
                //look for similar named files that also have references to this code            
                var items = new List<IProjectFile>();
                var pattern = string.Format(@"{0}\..*{1}", clrTypeClassName.ShortName, testClassSuffix);
                var finder = new ProjectFileFinder(items, new Regex(pattern));
                targetProjects.ForEach(p=>p.Project.Accept(finder));
                searchDomain = PsiShared.GetComponent<SearchDomainFactory>().CreateSearchDomain(items.Select(p => p.ToSourceFile()));
            }

#if R7
            var declarationsCache = solution.GetPsiServices().CacheManager.GetDeclarationsCache(DeclarationCacheLibraryScope.REFERENCED, true);
#else            
            var declarationsCache = solution.GetPsiServices().Symbols
                    .GetSymbolScope(LibrarySymbolScope.FULL, false, currentProject.GetResolveContext());
#endif

            ITypeElement declaredElement = declarationsCache.GetTypeElementByCLRName(clrTypeClassName);
                 
            var findReferences = services.Finder.FindReferences(
                declaredElement, searchDomain, new ProgressIndicator(textControl.Lifetime));

            List<IClassDeclaration> findReferencesWithinAssociatedAssembly = findReferences.Select(p => p.GetTreeNode().GetContainingNode<IClassDeclaration>(true)).ToList();
            return findReferencesWithinAssociatedAssembly
                .Select(p => p.DeclaredElement).ToList()                
                .Select(p => p as IClrDeclaredElement).ToList();                                    
        }               
    }
}

