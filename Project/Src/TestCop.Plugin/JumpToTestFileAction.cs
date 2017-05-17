// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2016
// --

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Menu;
using JetBrains.ReSharper.Features.Inspections.Bookmarks.NumberedBookmarks;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.UI.ActionsRevised;
using JetBrains.UI.PopupMenu;
using JetBrains.Util;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.TextControl.DataContext;
using JetBrains.UI.Avalon.TreeListView;

namespace TestCop.Plugin
{
    [Action("Jump to and from test file", Id = 92407, ShortcutScope = ShortcutScope.TextEditor, Icon = typeof(UnnamedThemedIcons.Agent16x16)
        //    , IdeaShortcuts = new []{"Control+G Control+T"}
        //    , VsShortcuts = new []{"Control+G Control+T"}
        )]
    public class JumpToTestFileAction : IExecutableAction, IInsertLast<NavigateGlobalGroup>
    {
        private Action<JetPopupMenus, JetPopupMenu, JetPopupMenu.ShowWhen> _menuDisplayer =
            (menus, menu, showWhen) =>
            {
                menus.Show(menu, showWhen);                
            };

        readonly Func<IClrDeclaredElement, IClrDeclaredElement, bool> _declElementMatcher =
                    (element, declaredElement) => B(element, declaredElement);

        private static bool B(IClrDeclaredElement element1, IClrDeclaredElement element2)
        {                
            var element1SoureFile = element1.GetSourceFiles().FirstOrDefault();
            var element2SourceFile = element2.GetSourceFiles().FirstOrDefault();

            if (element1SoureFile == null || element2SourceFile == null)
            {
                return element1.ToString() == element2.ToString(); 
            }

            return element1SoureFile.DisplayName == element2SourceFile.DisplayName;
        }

        /// <summary>
        /// For tesing 
        /// </summary>
        public static JumpToTestFileAction CreateWith(Action<JetPopupMenus, JetPopupMenu, JetPopupMenu.ShowWhen> overrideMenuDisplay)
        {
            return new JumpToTestFileAction{_menuDisplayer = overrideMenuDisplay};
        }
        
        bool IExecutableAction.Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            // fetch focused text editor control
            ITextControl textControl = context.GetData(TextControlDataConstants.TEXT_CONTROL);
            
            // enable this action if we are in text editor, disable otherwise            
            return textControl != null;
        }

        void IExecutableAction.Execute(IDataContext context, DelegateExecute nextExecute)
        {
            ITextControl textControl = context.GetData(TextControlDataConstants.TEXT_CONTROL);
            if (textControl == null)
            {
                MessageBox.ShowError("Text control unavailable.");                
                return;
            }
            ISolution solution = context.GetData(JetBrains.ProjectModel.DataContext.ProjectModelDataConstants.SOLUTION);
            if (solution == null){return;}
            
            IClrTypeName clrTypeClassName = ResharperHelper.GetClassNameAppropriateToLocation(solution, textControl);
            if (clrTypeClassName == null) return;

            var typeDeclaration = ResharperHelper.FindFirstCharpTypeDeclarationInDocument(solution, textControl.Document);
            if (typeDeclaration == null) return;
            
            var currentProject = context.GetData(JetBrains.ProjectModel.DataContext.ProjectModelDataConstants.Project);
            if (currentProject == null)
            {
                ResharperHelper.AppendLineToOutputWindow("Internal Error: No current project");
                return;
            }

            var targetProjects = currentProject.GetAssociatedProjects(textControl.ToProjectFile(solution));     
            if(targetProjects.IsEmpty())
            {
                ResharperHelper.AppendLineToOutputWindow("Unable to locate associated assembly - check project namespaces and testcop Regex");
                //ProjectMappingHelper.GetProjectMappingHeper().DumpDebug(solution);
                return;
            }
                       
            var settings = solution.GetPsiServices().SettingsStore
                .BindToContextTransient(ContextRange.Smart(textControl.ToDataContext()))                
                .GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);
                                                            
            var baseFileName = ResharperHelper.GetBaseFileName(context, solution);

            if (typeDeclaration.IsPartial && baseFileName.Contains("."))
            {
                baseFileName = baseFileName.Substring(0, baseFileName.LastIndexOf('.') );
            }

            bool isTestFile = baseFileName.EndsWith(settings.TestClassSuffixes());

            if (isTestFile != currentProject.IsTestProject())
            {                
                ResharperHelper.AppendLineToOutputWindow(
                            string.Format("Don't know how to navigate with '{0}' within project '{1}'"
                                , baseFileName, currentProject.Name));
                return;
            }
           
            var elementsFoundInTarget = new List<IClrDeclaredElement>();
            var elementsFoundInSolution = new List<IClrDeclaredElement>();

            foreach (var testClassSuffix in settings.GetAppropriateTestClassSuffixes(baseFileName))
            {
                var classNameFromFileName = ResharperHelper.UsingFileNameGetClassName(baseFileName)
                    .Flip(isTestFile, testClassSuffix);                

                if (clrTypeClassName != null)
                {
                    string className = clrTypeClassName.ShortName.Flip(isTestFile, testClassSuffix);
                    elementsFoundInTarget.AddRangeIfMissing(
                        ResharperHelper.FindClass(solution, className, targetProjects), _declElementMatcher);
                    
                    elementsFoundInSolution.AddRangeIfMissing(
                        ResharperHelper.FindClass(solution, className),_declElementMatcher);
                }
                
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
            IProject currentProject = context.GetData(JetBrains.ProjectModel.DataContext.ProjectModelDataConstants.PROJECT);

            var targetProjects = currentProject.GetAssociatedProjects(textControl.ToProjectFile(solution));
            ISearchDomain searchDomain;

            if (Settings.FindAnyUsageInTestAssembly)
            {
                searchDomain = PsiShared.GetComponent<SearchDomainFactory>().CreateSearchDomain(                
                targetProjects.SelectMany(proj=>proj.Project.GetAllProjectFiles().Select(p => p.GetPsiModule())) );
            }
            else
            {                
                ///TODO: investigate refactor and use regex pattern from targetProjects..
                //look for similar named files that also have references to this code            
                var items = new List<IProjectFile>();
                var pattern = string.Format(@"{0}\..*{1}", clrTypeClassName.ShortName, testClassSuffix);
                var finder = new ProjectFileFinder(items, new Regex(pattern));
                targetProjects.ForEach(p=>p.Project.Accept(finder));
                searchDomain = PsiShared.GetComponent<SearchDomainFactory>().CreateSearchDomain(items.Select(p => p.ToSourceFile()));
            }

            var declarationsCache = solution.GetPsiServices().Symbols
                    .GetSymbolScope(LibrarySymbolScope.FULL, false);//, currentProject.GetResolveContext());                    
            
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

