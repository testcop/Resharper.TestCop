// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2013
// --

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using JetBrains.Annotations;
using JetBrains.Application.Components;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings;
using JetBrains.Application.Threading;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.Application.UI.Controls.JetPopupMenu;
using JetBrains.DataFlow;
using JetBrains.IDE;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Properties;
using JetBrains.ReSharper.Daemon.Impl;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Projects;
using JetBrains.TextControl;
using JetBrains.TextControl.DataContext;
using JetBrains.Util;
using NUnit.Framework;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Tests
{
    [System.ComponentModel.Category("CSharp")]
    [TestFileExtension(".cs")]
    [TestFixture]
    public abstract class CSharpHighlightingWithinSolutionTestBase : BaseTest     
    {
        private ISolution _loadedTestSolution;

        protected ISolution LoadedTestSolution
        {
            get { return _loadedTestSolution; }            
        }

        protected abstract string SolutionName { get; }

        [CanBeNull]
        protected PsiLanguageType CompilerIdsLanguage
        {
            get { return CSharpLanguage.Instance; }
        }

        private TestSolutionManager SolutionManager
        {
            get
            {
                return ShellInstance.GetComponent<TestSolutionManager>();
            }
        }
        
        public override void TestFixtureSetUp()
        {            
            base.TestFixtureSetUp();
            RunGuarded(() =>
                           {
                               using (Locks.UsingWriteLock())
                               {
                                   FileSystemPath solutionFilePath = GetTestDataFilePath2(SolutionName);

                                   if (!solutionFilePath.ExistsFile)
                                   {
                                       Assert.Fail("Solution file doesn't exist: " + solutionFilePath);
                                   }

                                   _loadedTestSolution =                            
                                       SolutionManager.OpenExistingSolution(solutionFilePath);
                                   Assert.IsNotNull(_loadedTestSolution, "Failed to load solution " + solutionFilePath.FullPath);
                               }
                           });

        }
        
        public override void TestFixtureTearDown()
        {
            Assert.IsNotNull(_loadedTestSolution, "_loadedTestSolution == null");
            RunGuarded(() =>
                           {
                               ShellInstance.GetComponent<TestSolutionManager>().CloseSolution(_loadedTestSolution);
                               _loadedTestSolution = null;
                           });
           
            base.TestFixtureTearDown();
        }

        protected static void ClearRegExSettingsPriorToRun(IContextBoundSettingsStore settingsStore)
        {
            settingsStore.SetValue<TestFileAnalysisSettings, string>(
                s => s.TestProjectToCodeProjectNameSpaceRegEx, "NOT SET BY TEST");
            settingsStore.SetValue<TestFileAnalysisSettings, string>(
                s => s.TestProjectToCodeProjectNameSpaceRegExReplace, "NOT SET BY TEST");
            settingsStore.SetValue<TestFileAnalysisSettings, string>(
                s => s.TestProjectNameToCodeProjectNameRegEx, "NOT SET BY TEST");

            settingsStore.SetValue<TestFileAnalysisSettings, string>(
                s => s.SingleTestRegexTestToAssembly, "NOT SET BY TEST");
        
        }

        protected virtual TestHighlightingDumper CreateHighlightDumper(IPsiSourceFile sourceFile, TextWriter writer)
        {
            return new TestHighlightingDumper(sourceFile, writer, GetActiveStages(sourceFile.GetSolution()), HighlightingPredicate, CompilerIdsLanguage);
        }

        protected virtual IReadOnlyCollection<IDaemonStage> GetActiveStages(ISolution solution)
        {
            return DaemonStageManager.GetInstance(solution).Stages;
        }
           
        protected virtual bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile, IContextBoundSettingsStore settingsStore)
        {
            return true;
        }
   
        public void DoTestFiles(string fullProjectPathToTestFile)
        {
            Assert.IsNotNull(_loadedTestSolution, "Expected solution to be loaded");
            bool processedFile = false;

            var listOfProjectPaths = new List<string>();

            RunGuarded(() =>
                                {
                                    foreach (var project in _loadedTestSolution.GetAllProjects())
                                    {
                                        List<IProjectFile> projectFiles = project.GetAllProjectFiles().ToList();

                                        foreach (IProjectFile projectFile in
                                                projectFiles.Where(p => p.LanguageType.Is<CSharpProjectFileType>()))
                                        {
                                            listOfProjectPaths.Add(projectFile.GetPresentableProjectPath());                                            
                                            if (fullProjectPathToTestFile != projectFile.GetPresentableProjectPath())
                                                continue;
                                                                                       
                                            IPsiSourceFile sourceFile = projectFile.ToSourceFile();
                                            Assert.IsNotNull(sourceFile);
                                            
                                            if (!sourceFile.Properties.IsNonUserFile)
                                            {
                                                processedFile = true;
                                                Assert.IsTrue(projectFile.Kind == ProjectItemKind.PHYSICAL_FILE);

                                                IProjectFile file = projectFile;
                                                ExecuteWithGold(projectFile.Location.FullPath
                                                                , (writer =>
                                                                       {
                                                                           var highlightDumper =
                                                                               CreateHighlightDumper(sourceFile, writer);
                                                                           highlightDumper.DoHighlighting(
                                                                               DaemonProcessKind.VISIBLE_DOCUMENT);
                                                                           highlightDumper.Dump();

                                                                           DumperShortCutAction(file, writer);
                                                                       }));
                                                return;
                                            }
                                        }
                                    }
                                });
            if (!processedFile)
            {
                listOfProjectPaths.ForEach(f => Trace.WriteLine("Located Item:" + f));
                Assert.Fail("Failed to project file by project path " + fullProjectPathToTestFile);
            }
        }

        protected virtual void DumperShortCutAction(IProjectFile projectFile, TextWriter textwriter)
        {            
            Lifetime.Using((lifetime =>
                             {
                                var textControl = OpenTextControl(projectFile);
                                try
                                {                                                                           
                                var jumpToTestFileAction = GetShortcutAction(textwriter);
                                if(jumpToTestFileAction==null) return;

                                IDataContext context = DataContextOfTestTextControl.Create(lifetime,textControl, _loadedTestSolution);
                                
                                Func<Lifetime, DataContexts, IDataContext> dataContext = textControl.ToDataContext();

                                if ((jumpToTestFileAction).Update(context, new ActionPresentation(), null))
                                {                            
                                    (jumpToTestFileAction).Execute(context, null);                                    
                                }
                                }
                                finally
                                {
                                    CloseTextControl(textControl);
                                }
                                 
                }));             
             
        }

        private void CloseTextControl(ITextControl textControl)
        {            
            if(textControl != null)
                _loadedTestSolution.GetComponent<IEditorManager>().CloseTextControl(textControl, CloseTextControlSaveOptions.SaveIfDirty);
        }

        protected virtual IExecutableAction GetShortcutAction(TextWriter textwriter)
        {
            return null;
        }

        protected ITextControl OpenTextControl(IProjectFile projectFile, int? caretOffset = null)
        {
            Task<ITextControl> openProjectFileAsync = EditorManager.GetInstance(projectFile.GetSolution()).OpenProjectFileAsync(projectFile, new OpenFileOptions(true));
            openProjectFileAsync.Wait();
            return openProjectFileAsync.Result;
        }

        public Action<JetPopupMenus, JetPopupMenu, JetPopupMenu.ShowWhen> CreateJetPopMenuShowToWriterAction(TextWriter textWriter)
        {
           Action<JetPopupMenus, JetPopupMenu, JetPopupMenu.ShowWhen> menuDisplayer = (menus, menu, when) =>
            {
                foreach (var itm in Enumerable.ToList(menu.ItemKeys).Cast<SimpleMenuItem>())
                {
                    var s = itm.Text+ itm.ShortcutText ?? "";
                    
                    textWriter.WriteLine("[{0}] {1}",
                        ((JetBrains.Application.UI.Controls.RichTextAutomation)menu.Caption.Value).RichTextBlock.Value.Text
                        , s);                        
                }
            };

            return menuDisplayer;
        }
    }
        
}
