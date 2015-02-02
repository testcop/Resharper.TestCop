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

namespace TestCop.Plugin.Tests.MultipleTestProjectToSingleCodeProjectViaProjectName
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
            get { return @"MultipleTestProjectToSingleCodeProjectViaName\TestToClassNavigation"; }
        }

        protected override IActionHandler GetShortcutAction(TextWriter textwriter)
        {
            IActionHandler jumpToTestFileAction = new JumpToTestFileAction(CreateJetPopMenuShowToWriterAction(textwriter));
            return jumpToTestFileAction;
        }
        protected override string SolutionName
        {
            get { return @"MyCorp.TestApplication4.sln"; }
        }

        [Test]
        [TestCase(@"<APITests>\ClassATests.cs")]
        [TestCase(@"<APITests>\NS1\ClassATests.cs")]
        [TestCase(@"<APITests>\Properties\AssemblyInfo.cs")]
        public void Test(string testName)
        {
            const string altRegEx = "^(.*?)\\.?(Integration)*Tests$";

            ExecuteWithinSettingsTransaction((settingsStore =>
            {
                RunGuarded(
                    () =>
                    {
                        ClearRegExSettingsPriorToRun(settingsStore);

                        settingsStore.SetValue<TestFileAnalysisSettings, TestProjectStrategy>(
                            s => s.TestCopProjectStrategy, TestProjectStrategy.TestProjectHasSameNamespaceAsCodeProject);
                        settingsStore.SetValue<TestFileAnalysisSettings, bool>(
                            s => s.FindOrphanedProjectFiles, true);
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.TestProjectNameToCodeProjectNameRegEx, altRegEx);
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.TestProjectNameToCodeProjectNameRegExReplace, "$1");
                    }

                    );
                DoTestFiles(testName);
            }));
        }
    }
}
