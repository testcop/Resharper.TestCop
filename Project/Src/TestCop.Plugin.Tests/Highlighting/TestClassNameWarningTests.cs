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
    public class TestClassNameWarningTests : CSharpHighlightingTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile)
        {
            return highlighting is AbstractTestClassNameWarning;
        }

        protected override string RelativeTestDataPath
        {
            get { return @"highlighting\TestClassNameWarning"; }
        }

#if !R7
        public override void TestFixtureTearDown()
        {
            /* this logic is needed to undo the Resharper solution caching in place for tests
             * that break the more complex 'solution based tests' within the test solution */
            base.TestFixtureTearDown();

            RunGuarded(
                () => ShellInstance.GetComponent<TestSolutionManager>().CloseSolution());
//              () => ShellInstance.GetComponent<ReuseSolutionInTestsComponent>().CloseSolution());            
        }
#endif

        protected override string ProjectName
        {
            get { return "MyCorp.Project.Tests"; }
        }

        [Test]
        [TestCase("ClassAIsPartialTests.SomeText.cs")]
        [TestCase("ClassAIsPartialTests.cs")]
        [TestCase("ClassAIsPartial.SecurityTests.SomeText.cs")]
        [TestCase("ClassA.SomeCategoryTests.cs")]
        [TestCase("ClassA_SomeCategoryTests.cs")]
        [TestCase("ClassA.WithBDDTests.cs")]
        [TestCase("ClassATests.cs")]
        [TestCase("ClassBHasClassATests.cs")]
        public void TestsWithClassSuffixOfTests(string testName)
        {
            ExecuteWithinSettingsTransaction((settingsStore =>
                                              {
                                                  RunGuarded(
                                                      () =>
                                                      {
                                                          settingsStore
                                                              .SetValue<TestFileAnalysisSettings, TestProjectStrategy>(
                                                                  s => s.TestCopProjectStrategy,
                                                                  TestProjectStrategy
                                                                      .TestProjectHasSameNamespaceAsCodeProject);
                                                      }
                                                  );

                                                  DoTestSolution(testName);

                                              }));
        }

        [TestCase("TestClassWithDifferentRandomExt.cs")]
        [TestCase("ClassA.SomeCategoryWithDifferentRandomExt.cs")]
        [TestCase("ClassAMissingSuffix.cs")]
        [TestCase("ClassAMissingSuffixDisabled.cs")]
        public void TestsWithClassSuffixDifferentToDefault(string testName)
        {
            // the default suffix is 'Tests' - we test that this can be overidden 

            ExecuteWithinSettingsTransaction((settingsStore =>
                                              {
                                                  RunGuarded(
                                                      () =>
                                                      {
                                                          settingsStore
                                                              .SetValue<TestFileAnalysisSettings, TestProjectStrategy>(
                                                                  s => s.TestCopProjectStrategy,
                                                                  TestProjectStrategy
                                                                      .TestProjectHasSameNamespaceAsCodeProject);
                                                          settingsStore.SetValue<TestFileAnalysisSettings, string>(
                                                              s => s.TestClassSuffix,
                                                              "RandomExt");
                                                      }
                                                  );
                                                  DoTestSolution(testName);
                                              }));
        }
    }
}
