// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System.IO;
using JetBrains.ActionManagement;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using NUnit.Framework;

namespace TestCop.Plugin.Tests.MultipleTestProjectToSingleCodeProjectViaNamespace
{
    [TestFixture]
    public class TestToClassFileNavigationTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        {
            return highlighting.GetType().Namespace.Contains("TestCop");
        }

        protected override string RelativeTestDataPath
        {
            get { return @"MultipleTestProjectToSingleCodeProject\TestToClassNavigation"; }
        }

        protected override IActionHandler GetShortcutAction(TextWriter textwriter)
        {
            IActionHandler jumpToTestFileAction = new JumpToTestFileAction(CreateJetPopMenuShowToWriterAction(textwriter));
            return jumpToTestFileAction;
        }
        protected override string SolutionName
        {
            get { return @"TestApplication.sln"; }
        }

        [Test]
        [TestCase(@"<TestApplication2Tests>\NS2\ClassGTests.cs")]
        [TestCase(@"<TestApplication2Tests>\Properties\AssemblyInfo.cs")]
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
            }));
        }
    }
}
