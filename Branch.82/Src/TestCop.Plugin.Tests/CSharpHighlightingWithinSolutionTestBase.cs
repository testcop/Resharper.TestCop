// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Annotations;
using JetBrains.Application.Components;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.Impl;
using JetBrains.ReSharper.Daemon.Test;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Services.ValueTracking;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework.ProjectModel;
using JetBrains.TestShell.Infra;
using JetBrains.TextControl;
using JetBrains.UI.PopupMenu;
using JetBrains.Util;
using NUnit.Framework;

namespace TestCop.Plugin.Tests
{        
    [System.ComponentModel.Category("CSharp")]
    [TestFileExtension(".cs")]
    [TestFixture]
    public abstract class CSharpHighlightingWithinSolutionTestBase : BaseTest     
    {
        private ISolution _loadedTestSolution;

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

        protected void ClearRegExSettingsPriorToRun(IContextBoundSettingsStore settingsStore)
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

        protected virtual IList<IDaemonStage> GetActiveStages(ISolution solution)
        {
            return DaemonStageManager.GetInstance(solution).Stages;
        }
           
        protected virtual bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
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
                listOfProjectPaths.ForEach(f => System.Diagnostics.Trace.WriteLine("Located Item:" + f));
                Assert.Fail("Failed to project file by project path " + fullProjectPathToTestFile);
            }
        }

        protected virtual void DumperShortCutAction(IProjectFile projectFile, TextWriter textwriter)
        {            
            Lifetimes.Using((lifetime =>
                {                                    
                    using (ITextControl textControl = OpenTextControl(projectFile))
                    {
                        var jumpToTestFileAction = GetShortcutAction(textwriter);
                        if(jumpToTestFileAction==null) return;

                        IDataContext context = DataContextOfTestTextControl.Create(lifetime,textControl, _loadedTestSolution);
                        
                        if ((jumpToTestFileAction).Update(context, new ActionPresentation(), null))
                        {                            
                            (jumpToTestFileAction).Execute(context, null);                                    
                        }                        
                    }
                }));
        }

        protected virtual IActionHandler GetShortcutAction(TextWriter textwriter)
        {
            return null;
        }

        protected ITextControl OpenTextControl(IProjectFile projectFile, int? caretOffset = null)
        {            
            ITextControl openProjectFile = EditorManager.GetInstance(projectFile.GetSolution()).OpenProjectFile(projectFile, true);
            return openProjectFile;
        }

        public Action<JetPopupMenu, JetPopupMenu.ShowWhen> CreateJetPopMenuShowToWriterAction(TextWriter textWriter)
        {
           Action<JetPopupMenu, JetPopupMenu.ShowWhen> menuDisplayer = (menu, when) =>
            {
                foreach (var itm in Enumerable.ToList(menu.ItemKeys).Cast<SimpleMenuItem>())
                {
                    var s = itm.Text+ itm.ShortcutText ?? "";
                    textWriter.WriteLine("[{0}] {1}", menu.Caption.Value, s);
                }
            };

            return menuDisplayer;
        }
    }
        
}
