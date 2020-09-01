// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2018
// --

using System;
using System.IO;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.Application.Threading;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers.Transactions;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.Util;
using NUnit.Framework;
using TestCop.Plugin.Helper;
using TestCop.Plugin.Highlighting;
using TestCop.Plugin.QuickFixActions;

namespace TestCop.Plugin.Tests.MultipleTestProjectToSingleCodeProjectViaNamespace
{
    [TestFixture]
    public class MoveTestFiletoCorrectLocationTests : CSharpHighlightingWithinSolutionTestBase
    {
        private Action actionToRun;

        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile, IContextBoundSettingsStore settingsStore)
        {
            if (highlighting is TestFileNameSpaceWarning)
            {                                
                var fixer = new MoveFileBulbItem(highlighting as TestFileNameSpaceWarning);

                IProjectFile projectFile = sourceFile.ToProjectFile();
                var textControl = base.OpenTextControl(projectFile);


                actionToRun = ()=> fixer.Execute(LoadedTestSolution, textControl);                
                return true;                                 
            }
            return false;
        }

        protected override string RelativeTestDataPath
        {
            get { return @"MultipleTestProjectToSingleCodeProject\TestToClassNavigation"; }
        }

        protected override IExecutableAction GetShortcutAction(TextWriter textwriter)
        {
            IExecutableAction jumpToTestFileAction = TestCopJumpToTestFileAction.CreateWith(CreateJetPopMenuShowToWriterAction(textwriter));
            return jumpToTestFileAction;
        }
        protected override string SolutionName
        {
            get { return @"TestApplication.sln"; }
        }

        [Test]
        [TestCase(@"<TestApplication2Tests>\ClassD.WrongFolderTests.cs")]
        public void Test(string testName)
        {
            const string altRegEx = "^(.*?)\\.?(Integration)*Tests$";

            ExecuteWithinSettingsTransaction((settingsStore =>
            {                
                RunGuarded(
                    () =>
                    {
                        ClearRegExSettingsPriorToRun(settingsStore);

                        settingsStore.SetValue<TestFileAnalysisSettings, bool>(
                            s => s.FindOrphanedProjectFiles, true);
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.TestProjectToCodeProjectNameSpaceRegEx, altRegEx);
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.TestProjectToCodeProjectNameSpaceRegExReplace, "$1");
                    }

                    );
                DoTestFiles(testName);

                Lifetime.Using(lifetime =>
                                {
                                    LoadedTestSolution.Locks.ExecuteWithReadLock(() =>
                                                                                 {
                                                                                     using (var cookie =
                                                                                         LoadedTestSolution.CreateTransactionCookie(DefaultAction.Rollback,
                                                                                                 this.GetType().Name,new ProgressIndicator(lifetime)))
                                                                                     {

                                                                                         ResharperHelper.ProtectActionFromReEntry(lifetime, "X",actionToRun);
                                                                                     }
                                                                                 });
            });
            }));
        }
    }
}
