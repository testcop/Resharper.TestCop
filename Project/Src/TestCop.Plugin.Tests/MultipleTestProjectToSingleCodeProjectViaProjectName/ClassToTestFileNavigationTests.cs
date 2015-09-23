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

namespace TestCop.Plugin.Tests.MultipleTestProjectToSingleCodeProjectViaProjectName
{    
    [TestFixture]
    public class ClassToTestFileNavigationTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile)
        {
            return highlighting.GetType().Namespace.Contains("TestCop");
        }

        protected override string RelativeTestDataPath
        {
            get { return @"MultipleTestProjectToSingleCodeProjectViaName\ClassToTestNavigation"; }
        }

        protected override IExecutableAction GetShortcutAction(TextWriter textwriter)
        {
            var jumpToTestFileAction = JumpToTestFileAction.CreateWith(CreateJetPopMenuShowToWriterAction(textwriter));
            return jumpToTestFileAction;
        }
        protected override string SolutionName
        {
            get { return @"MyCorp.TestApplication4.sln"; }
        }

        [Test]
        [TestCase(@"<API>\ClassA.cs")]
        [TestCase(@"<API>\NS1\APIClassBWithNoTest.cs")]
        [TestCase(@"<API>\NS1\ClassA.cs")]
        [TestCase(@"<API>\Properties\AssemblyInfo.cs")]     
        public void Test(string testName)
        {            
            ExecuteWithinSettingsTransaction((settingsStore =>
            {
                RunGuarded(
                    () =>
                    {                        
                        SetupTestCopSettings(settingsStore);
                    }
                    );
                DoTestFiles(testName);
            }));
        }

        internal static void SetupTestCopSettings(IContextBoundSettingsStore settingsStore)
        {
            ClearRegExSettingsPriorToRun(settingsStore);

            const string altRegEx = "^(.*?)\\.?(Integration)*Tests$";
            settingsStore.SetValue<TestFileAnalysisSettings, TestProjectStrategy>(
                s => s.TestCopProjectStrategy, TestProjectStrategy.TestProjectHasSameNamespaceAsCodeProject);
            settingsStore.SetValue<TestFileAnalysisSettings, bool>(
                s => s.FindOrphanedProjectFiles, true);
            settingsStore.SetValue<TestFileAnalysisSettings, string>(
                s => s.TestClassSuffix, "Tests,IntegrationTests");

            settingsStore.SetValue<TestFileAnalysisSettings, string>(
                s => s.TestProjectNameToCodeProjectNameRegEx, altRegEx);
            settingsStore.SetValue<TestFileAnalysisSettings, string>(
                s => s.TestProjectNameToCodeProjectNameRegExReplace, "");
        }
    }
}
