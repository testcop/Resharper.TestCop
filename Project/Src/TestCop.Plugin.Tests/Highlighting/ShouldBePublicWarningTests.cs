// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2015
// --

using JetBrains.Application.Components;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.FeaturesTestFramework.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.TestFramework.Projects;
using NUnit.Framework;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.Highlighting
{
    [TestFixture]
    public class ShouldBePublicWarningTests : CSharpHighlightingTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile)        
        {
            return highlighting is AbstractShouldBePublicWarning;
        }
       
        protected override string RelativeTestDataPath
        {
            get { return @"highlighting\ShouldBePublicWarning"; }
        }

        #if !R7
        public override void TestFixtureTearDown()
        {
            /* this logic is needed to undo the Resharper solution caching in place for tests
             * that break the more complex 'solution based tests' within the test solution */
            base.TestFixtureTearDown();

            RunGuarded(
              () => ShellInstance.GetComponent<TestSolutionManager>().CloseSolution());              
        }
        #endif

        protected override string ProjectName
        {
            get
            {
                return "MyCorp.Project.Tests";
            }
        }

        [Test]        
        [TestCase("PrivateNUnitTestClass.cs")]
        [TestCase("PrivateNUnitTestClassDisabled.cs")]
        [TestCase("PrivateMSTestTestMethod.cs")]
        [TestCase("PrivateNUnitTestMethod.cs")]
        [TestCase("PrivateXUnitTestMethod.cs")]
        public void Test(string testName)
        {            
            ExecuteWithinSettingsTransaction((settingsStore =>
            {
                RunGuarded(
                    () =>{
                        settingsStore.SetValue<TestFileAnalysisSettings, TestProjectStrategy>(
                            s => s.TestCopProjectStrategy, TestProjectStrategy.TestProjectHasSameNamespaceAsCodeProject);
                    }
                    );
                DoTestSolution(testName);
            }));
        }
    }
}
