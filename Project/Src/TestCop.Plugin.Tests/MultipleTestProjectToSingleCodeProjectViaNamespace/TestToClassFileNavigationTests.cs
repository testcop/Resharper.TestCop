// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System.IO;
using JetBrains.ActionManagement;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.ActionsRevised;
using NUnit.Framework;

namespace TestCop.Plugin.Tests.MultipleTestProjectToSingleCodeProjectViaNamespace
{
    [TestFixture]
    public class TestToClassFileNavigationTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile)
        {
            return highlighting.GetType().Namespace.Contains("TestCop");
        }

        protected override string RelativeTestDataPath
        {
            get { return @"MultipleTestProjectToSingleCodeProject\TestToClassNavigation"; }
        }

        protected override IExecutableAction GetShortcutAction(TextWriter textwriter)
        {
            IExecutableAction jumpToTestFileAction = JumpToTestFileAction.CreateWith(CreateJetPopMenuShowToWriterAction(textwriter));
            return jumpToTestFileAction;
        }
        protected override string SolutionName
        {
            get { return @"TestApplication.sln"; }
        }

        [Test]
        [TestCase(@"<TestApplication2Tests>\ClassATests.cs")]
        [TestCase(@"<TestApplication2Tests>\NS1\ClassForTestingPartialTests.cs")]
        [TestCase(@"<TestApplication2Tests>\NS1\ClassForTestingPartialTests.partial.cs")]
        [TestCase(@"<TestApplication2Tests>\NS1\ClassForTestingPartial.SecurityTests.partial.cs")]
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
