// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2020
// --

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.Application.Shell;
using JetBrains.Application.Shortcuts.ShortcutManager;
using JetBrains.Application.Threading;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.Application.UI.ActionSystem.ActionsRevised.Menu;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Menu;
using JetBrains.ReSharper.Features.Inspections.Bookmarks.NumberedBookmarks;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.TextControl.DataContext;
using JetBrains.Util;

using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;

namespace TestCop.Plugin
{
    [Action("Jump to and from test file", Id = 92407, ShortcutScope = ShortcutScope.TextEditor, Icon = typeof(UnnamedThemedIcons.Agent16x16)
        , IdeaShortcuts = new []{"Control+G Control+T"}, VsShortcuts = new []{"Control+G Control+T"}
    )]
    public class TestCopJumpToTestFileAction : IExecutableAction, IInsertLast<NavigateGlobalGroup>
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
        public static TestCopJumpToTestFileAction CreateWith(Action<JetPopupMenus, JetPopupMenu, JetPopupMenu.ShowWhen> overrideMenuDisplay)
        {
            return new TestCopJumpToTestFileAction{_menuDisplayer = overrideMenuDisplay};
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
                ResharperHelper.AppendLineToOutputWindow(solution.Locks, "Internal Error: No current project");
                return;
            }

            var targetProjects = currentProject.GetAssociatedProjects(textControl.ToProjectFile(solution));     
            if(targetProjects.IsEmpty())
            {
                ResharperHelper.AppendLineToOutputWindow(solution.Locks, "Unable to locate associated assembly - check project namespaces and testcop Regex");
                //ProjectMappingHelper.GetProjectMappingHeper().DumpDebug(solution);
                return;
            }
                       
            var settings = solution.GetPsiServices().SettingsStore
                .BindToContextTransient(ContextRange.Smart(textControl.ToDataContext()))                
                .GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);
                                                            
            var baseFileName = ResharperHelper.GetBaseFileName(context, solution);
            
            bool isTestFile = baseFileName.EndsWith(settings.TestClassSuffixes());

            if (isTestFile != currentProject.IsTestProject())
            {                
                ResharperHelper.AppendLineToOutputWindow(solution.Locks,
                            string.Format("Don't know how to navigate with '{0}' within project '{1}'. It is a {2} file within a {3} project"
                                , baseFileName, currentProject.Name, isTestFile ? "test" : "code", currentProject.IsTestProject() ? "test" : "code"));
                return;
            }
           
            var elementsFoundInTarget = new List<IClrDeclaredElement>();
            var elementsFoundInSolution = new List<IClrDeclaredElement>();
          

            foreach (var singleTargetProject in targetProjects)
            {                
                foreach (var patternMatcher in singleTargetProject.FilePattern)
                {
                    //FindByClassName      
                    elementsFoundInSolution.AddRangeIfMissing(ResharperHelper.FindClass(solution, patternMatcher.RegEx.ToString()), _declElementMatcher);
                    elementsFoundInTarget.AddRangeIfMissing(ResharperHelper.FindClass(solution, patternMatcher.RegEx.ToString(), new List<IProject>() { singleTargetProject.Project }), _declElementMatcher);
                    
                     if (!isTestFile)
                     {
                         //Find via filename (for when we switch to test files)
                         var otherMatches = ResharperHelper.FindFirstTypeWithinCodeFiles(solution, patternMatcher.RegEx, singleTargetProject.Project);
                         elementsFoundInTarget.AddRangeIfMissing(otherMatches, _declElementMatcher);
                     }                     
                }                               
            }

            if (!isTestFile)
            {
                var references = FindReferencesWithinAssociatedAssembly(context, solution, textControl, clrTypeClassName, targetProjects);
                elementsFoundInTarget.AddRangeIfMissing(references, _declElementMatcher);               
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

        private IList<IClrDeclaredElement> FindReferencesWithinAssociatedAssembly(IDataContext context, ISolution solution, ITextControl textControl
            , IClrTypeName clrTypeClassName, IList<TestCopProjectItem> targetProjects)
        {
            if (clrTypeClassName == null)
            {
                ResharperHelper.AppendLineToOutputWindow(solution.Locks, "FindReferencesWithinAssociatedAssembly() - clrTypeClassName was null");
                return new List<IClrDeclaredElement>();
            }

            IPsiServices services = solution.GetPsiServices();
            
            ISearchDomain searchDomain;

            if (Settings.FindAnyUsageInTestAssembly)
            {
                searchDomain = PsiShared.GetComponent<SearchDomainFactory>().CreateSearchDomain(                
                targetProjects.SelectMany(proj=>proj.Project.GetAllProjectFiles().Select(p => p.GetPsiModule())) );
            }
            else
            {                     
                //look for similar named files that also have references to this code            
                var items = new List<ProjectFileFinder.Match>();                                
                targetProjects.ForEach(p=>p.Project.Accept(new ProjectFileFinder(items, p.FilePattern)));
                searchDomain = PsiShared.GetComponent<SearchDomainFactory>().CreateSearchDomain(items.Select(p => p.ProjectFile.ToSourceFile()));
            }

            var declarationsCache = solution.GetPsiServices().Symbols
                    .GetSymbolScope(LibrarySymbolScope.NONE, false);//, currentProject.GetResolveContext());                    
                        
            ITypeElement declaredElement = declarationsCache.GetTypeElementByCLRName(clrTypeClassName);
            
            var findReferences = services.Finder.FindReferences(declaredElement, searchDomain, new ProgressIndicator(textControl.Lifetime));

            List<IClassDeclaration> findReferencesWithinAssociatedAssembly = findReferences.Select(p => p.GetTreeNode().GetContainingNode<IClassDeclaration>(true)).ToList();
            return findReferencesWithinAssociatedAssembly
                .Select(p => p.DeclaredElement).ToList()                
                .Select(p => p as IClrDeclaredElement).ToList();                                    
        }
    }
}

