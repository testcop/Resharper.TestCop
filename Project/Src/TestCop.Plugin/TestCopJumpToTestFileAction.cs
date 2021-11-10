// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2020
// --

namespace TestCop.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Application.DataContext;
    using JetBrains.Application.Progress;
    using JetBrains.Application.Settings;
    using JetBrains.Application.Shortcuts.ShortcutManager;
    using JetBrains.Application.UI.Actions;
    using JetBrains.Application.UI.ActionsRevised.Menu;
    using JetBrains.Application.UI.ActionSystem.ActionsRevised.Menu;
    using JetBrains.Application.UI.Controls.JetPopupMenu;
    using JetBrains.Metadata.Reader.API;
    using JetBrains.ProjectModel;
    using JetBrains.ProjectModel.DataContext;
    using JetBrains.ReSharper.Feature.Services.Menu;
    using JetBrains.ReSharper.Features.Inspections.Bookmarks.NumberedBookmarks;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Caches;
    using JetBrains.ReSharper.Psi.CSharp.Tree;
    using JetBrains.ReSharper.Psi.Modules;
    using JetBrains.ReSharper.Psi.Resolve;
    using JetBrains.ReSharper.Psi.Search;
    using JetBrains.ReSharper.Resources.Shell;
    using JetBrains.TextControl;
    using JetBrains.TextControl.DataContext;
    using JetBrains.Util;

    using TestCop.Plugin.Extensions;
    using TestCop.Plugin.Helper;

    [Action("Jump to and from test file", Id = 92407, ShortcutScope = ShortcutScope.TextEditor,
        Icon = typeof(UnnamedThemedIcons.Agent16x16), IdeaShortcuts = new[] { "Control+G Control+T" },
        VsShortcuts = new[] { "Control+G Control+T" })]
    public class TestCopJumpToTestFileAction : IExecutableAction, IInsertLast<NavigateGlobalGroup>
    {
        private Action<JetPopupMenus, JetPopupMenu, JetPopupMenu.ShowWhen> _menuDisplayer =
            (menus, menu, showWhen) => { menus.Show(menu, showWhen); };

        private readonly Func<IClrDeclaredElement, IClrDeclaredElement, bool> _declElementMatcher = B;

        private static bool B(IDeclaredElement element1, IDeclaredElement element2)
        {
            IPsiSourceFile element1SourceFile = element1.GetSourceFiles().FirstOrDefault();
            IPsiSourceFile element2SourceFile = element2.GetSourceFiles().FirstOrDefault();

            if (element1SourceFile == null || element2SourceFile == null)
            {
                return element1.ToString() == element2.ToString();
            }

            return element1SourceFile.DisplayName == element2SourceFile.DisplayName;
        }

        /// <summary>
        /// For testing
        /// </summary>
        public static TestCopJumpToTestFileAction CreateWith(
            Action<JetPopupMenus, JetPopupMenu, JetPopupMenu.ShowWhen> overrideMenuDisplay)
        {
            return new TestCopJumpToTestFileAction { _menuDisplayer = overrideMenuDisplay };
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

            ISolution solution = context.GetData(ProjectModelDataConstants.SOLUTION);

            if (solution == null)
            {
                return;
            }

            IClrTypeName clrTypeClassName = ResharperHelper.GetClassNameAppropriateToLocation(solution, textControl);

            if (clrTypeClassName == null)
            {
                return;
            }

            ICSharpTypeDeclaration typeDeclaration =
                ResharperHelper.FindFirstCharpTypeDeclarationInDocument(solution, textControl.Document);

            if (typeDeclaration == null)
            {
                return;
            }

            IProject currentProject = context.GetData(ProjectModelDataConstants.PROJECT);

            if (currentProject == null)
            {
                ResharperHelper.AppendLineToOutputWindow(solution.Locks, "Internal Error: No current project");
                return;
            }

            IList<TestCopProjectItem> targetProjects = currentProject.GetAssociatedProjects(textControl.ToProjectFile(solution));

            if (targetProjects.IsEmpty())
            {
                ResharperHelper.AppendLineToOutputWindow(solution.Locks,
                    "Unable to locate associated assembly - check project namespaces and testcop Regex");

                //ProjectMappingHelper.GetProjectMappingHeper().DumpDebug(solution);
                return;
            }

            TestFileAnalysisSettings settings = solution.GetPsiServices().SettingsStore
                .BindToContextTransient(ContextRange.Smart(textControl.ToDataContext()))
                .GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);

            string baseFileName = ResharperHelper.GetBaseFileName(context, solution);

            bool isTestFile = baseFileName.EndsWith(settings.TestClassSuffixes());

            if (isTestFile != currentProject.IsTestProject())
            {
                ResharperHelper.AppendLineToOutputWindow(solution.Locks,
                    string.Format(
                        "Don't know how to navigate with '{0}' within project '{1}'. It is a {2} file within a {3} project"
                        , baseFileName, currentProject.Name, isTestFile
                            ? "test"
                            : "code", currentProject.IsTestProject()
                            ? "test"
                            : "code"));
                return;
            }

            List<IClrDeclaredElement> elementsFoundInTarget = new List<IClrDeclaredElement>();
            List<IClrDeclaredElement> elementsFoundInSolution = new List<IClrDeclaredElement>();

            foreach (TestCopProjectItem singleTargetProject in targetProjects)
            {
                foreach (TestCopProjectItem.FilePatternMatcher patternMatcher in singleTargetProject.FilePattern)
                {
                    // FindByClassName
                    elementsFoundInSolution.AddRangeIfMissing(
                        ResharperHelper.FindClass(solution, patternMatcher.RegEx.ToString()), this._declElementMatcher);
                    elementsFoundInTarget.AddRangeIfMissing(
                        ResharperHelper.FindClass(solution, patternMatcher.RegEx.ToString(),
                            new List<IProject> { singleTargetProject.Project }), this._declElementMatcher);

                    if (!isTestFile)
                    {
                        //Find via filename (for when we switch to test files)
                        IEnumerable<ITypeElement> otherMatches =
                            ResharperHelper.FindFirstTypeWithinCodeFiles(solution, patternMatcher.RegEx,
                                singleTargetProject.Project);
                        elementsFoundInTarget.AddRangeIfMissing(otherMatches, this._declElementMatcher);
                    }
                }
            }

            if (!isTestFile)
            {
                IEnumerable<IClrDeclaredElement> references =
                    this.FindReferencesWithinAssociatedAssembly(context, solution, textControl, clrTypeClassName, targetProjects);
                elementsFoundInTarget.AddRangeIfMissing(references, this._declElementMatcher);
            }

            JumpToTestMenuHelper.PromptToOpenOrCreateClassFiles(this._menuDisplayer, textControl.Lifetime, context,
                solution
                , currentProject, clrTypeClassName, targetProjects
                , elementsFoundInTarget, elementsFoundInSolution);
        }

        private TestFileAnalysisSettings Settings
        {
            get
            {
                ISettingsStore settingsStore = Shell.Instance.GetComponent<ISettingsStore>();
                IContextBoundSettingsStore contextBoundSettingsStore =
                    settingsStore.BindToContextTransient(ContextRange.ApplicationWide);
                TestFileAnalysisSettings mySettings =
                    contextBoundSettingsStore.GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);
                return mySettings;
            }
        }

        private IEnumerable<IClrDeclaredElement> FindReferencesWithinAssociatedAssembly(IDataContext context,
            ISolution solution, ITextControl textControl, IClrTypeName clrTypeClassName,
            IEnumerable<TestCopProjectItem> targetProjects)
        {
            if (clrTypeClassName == null)
            {
                ResharperHelper.AppendLineToOutputWindow(solution.Locks,
                    "FindReferencesWithinAssociatedAssembly() - clrTypeClassName was null");
                return new List<IClrDeclaredElement>();
            }

            IPsiServices services = solution.GetPsiServices();

            ISearchDomain searchDomain;

            if (this.Settings.FindAnyUsageInTestAssembly)
            {
                searchDomain = PsiShared.GetComponent<SearchDomainFactory>().CreateSearchDomain(
                    targetProjects.SelectMany(proj => proj.Project.GetAllProjectFiles().Select(p => p.GetPsiModule())));
            }
            else
            {
                // look for similar named files that also have references to this code
                List<ProjectFileFinder.Match> items = new List<ProjectFileFinder.Match>();

                foreach (TestCopProjectItem projectItem in targetProjects)
                {
                    projectItem.Project.Accept(new ProjectFileFinder(items, projectItem.FilePattern));
                }

                searchDomain = PsiShared.GetComponent<SearchDomainFactory>()
                    .CreateSearchDomain(items.Select(p => p.ProjectFile.ToSourceFile()));
            }

            ISymbolScope declarationsCache = solution.GetPsiServices().Symbols
                .GetSymbolScope(LibrarySymbolScope.NONE, false); //, currentProject.GetResolveContext());

            ITypeElement declaredElement = declarationsCache.GetTypeElementByCLRName(clrTypeClassName);

            IReference[] findReferences =
                services.Finder.FindReferences(declaredElement, searchDomain, new ProgressIndicator(textControl.Lifetime));

            List<IClassDeclaration> findReferencesWithinAssociatedAssembly =
                findReferences.Select(p => p.GetTreeNode().GetContainingNode<IClassDeclaration>(true)).ToList();
            return findReferencesWithinAssociatedAssembly
                .Select(p => p.DeclaredElement).ToList()
                .Select(p => p as IClrDeclaredElement).ToList();
        }
    }
}