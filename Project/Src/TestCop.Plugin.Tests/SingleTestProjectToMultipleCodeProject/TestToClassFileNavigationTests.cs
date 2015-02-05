// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System.IO;
using System.Text.RegularExpressions;
using JetBrains.ActionManagement;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.ActionsRevised;
using NUnit.Framework;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.SingleTestProjectToMultipleCodeProject
{
    [TestFixture]
    public class TestToClassFileNavigationTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile)
        {
            return highlighting is TestFileNameSpaceWarning;
        }

        protected override string RelativeTestDataPath
        {
            get { return @"SingleTestProjectForManyCodeProject\TestToClassNavigation"; }
        }

        protected override IExecutableAction GetShortcutAction(TextWriter textwriter)
        {
            var jumpToTestFileAction = JumpToTestFileAction.CreateWith(CreateJetPopMenuShowToWriterAction(textwriter));
            return jumpToTestFileAction;
        }
        protected override string SolutionName
        {
            get { return @"TestApplication3.sln"; }
        }


        [Test]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\API\ClassATests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\API\NS1\ClassATests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\API\NS1\NS2\ClassCTests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\API\NS1\NS2\ClassATests.cs")]        
        [TestCase(@"<MyCorp.TestApplication3.Tests>\ClassNotValidAtRootTests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\DAL\ClassATests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\DAL\NS1\ClassATests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\DAL\NS1\NS2\ClassATests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\DAL\NS1\NS2\DALClassDWithNoCodeTests.cs")]        
        public void Test(string testName)
        {   
             // http://myregexp.com/
        
            ExecuteWithinSettingsTransaction((settingsStore =>
            {
                RunGuarded(
                    () =>
                    {
                        ClearRegExSettingsPriorToRun(settingsStore);

                        settingsStore.SetValue<TestFileAnalysisSettings, TestProjectStrategy>(
                            s => s.TestCopProjectStrategy, TestProjectStrategy.SingleTestProjectPerSolution );

                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.SingleTestRegexTestToAssembly, RegExTests.RegexForSingleTestProjectStrategy);
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.SingleTestRegexTestToAssemblyProjectReplace, @"$1$2");
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.SingleTestRegexTestToAssemblyProjectSubNamespaceReplace, @"$3");
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.SingleTestRegexCodeToTestAssembly, @"NOT REQ FOR TEST");
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.SingleTestRegexCodeToTestReplace, @"NOT REQ FOR TEST"); 
                    }

                    );
                DoTestFiles(testName);
            }));
        }
    }
}
