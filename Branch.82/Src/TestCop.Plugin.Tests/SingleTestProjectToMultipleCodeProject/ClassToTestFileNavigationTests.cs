// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System.IO;
using JetBrains.ActionManagement;
using JetBrains.Application.Components;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.TestFramework.ProjectModel;
using NUnit.Framework;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.SingleTestProjectToMultipleCodeProject
{    
    [TestFixture]
    public class ClassToTestFileNavigationTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        {
            return highlighting is TestFileNameSpaceWarning;
        }

        protected override string RelativeTestDataPath
        {
            get { return @"SingleTestProjectForManyCodeProject\ClassToTestNavigation"; }
        }

        protected override IActionHandler GetShortcutAction(TextWriter textwriter)
        {
            IActionHandler jumpToTestFileAction = new JumpToTestFileAction(CreateJetPopMenuShowToWriterAction(textwriter));
            return jumpToTestFileAction;
        }
        protected override string SolutionName
        {
            get { return @"TestApplication3.sln"; }
        }
        
        [Test]
        [TestCase(@"<MyCorp.TestApplication3.API>\ClassA.cs")]
        [TestCase(@"<MyCorp.TestApplication3.API>\NS1\APIClassBWithNoTest.cs")]
        [TestCase(@"<MyCorp.TestApplication3.API>\NS1\ClassA.cs")]
        [TestCase(@"<MyCorp.TestApplication3.API>\NS1\NS2\ClassA.cs")]
        [TestCase(@"<MyCorp.TestApplication3.API>\NS1\NS2\ClassC.cs")]

        [TestCase(@"<MyCorp.TestApplication3.DAL>\ClassA.cs")]
        [TestCase(@"<MyCorp.TestApplication3.DAL>\DALClassCWithNoTest.cs")]
        [TestCase(@"<MyCorp.TestApplication3.DAL>\NS1\ClassA.cs")]
        [TestCase(@"<MyCorp.TestApplication3.DAL>\NS1\NS2\ClassA.cs")]         
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
                          s => s.TestCopProjectStrategy, TestProjectStrategy.SingleTestProjectPerSolution);

                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.SingleTestRegexTestToAssembly, RegExTests.RegexForSingleTestProjectStrategy);
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.SingleTestRegexTestToAssemblyProjectReplace, @"$1$2");
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.SingleTestRegexTestToAssemblyProjectSubNamespaceReplace, @"$3");
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.SingleTestRegexCodeToTestAssembly, @"^(.*?\..*?)(\..*?)$");
                        settingsStore.SetValue<TestFileAnalysisSettings, string>(
                            s => s.SingleTestRegexCodeToTestReplace, @"$2"); 
                    }
                    
                    );
                DoTestFiles(testName);
            }));
        }
    }
}
