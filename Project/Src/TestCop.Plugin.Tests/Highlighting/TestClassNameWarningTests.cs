// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System;
using JetBrains.Application.Components;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp;
using JetBrains.TestFramework.ProjectModel;
using NUnit.Framework;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.Highlighting
{
    [TestFixture]
    public class TestClassNameWarningTests : CSharpHighlightingTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
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
              () => ShellInstance.GetComponent<ReuseSolutionInTestsComponent>().CloseSolution());            
        }
        #endif

        [Test]
        [TestCase("ClassA.SomeCategoryTests.cs")]
        [TestCase("ClassA_SomeCategoryTests.cs")]
        [TestCase("ClassA.WithBDDTests.cs")]
        [TestCase("ClassATests.cs")]
        [TestCase("ClassBHasClassATests.cs")]        
        public void TestsWithClassSuffixOfTests(string testName)
        {
            DoTestFiles(testName);
        }

        [TestCase("TestClassWithDifferentRandomExt.cs")]
        [TestCase("ClassA.SomeCategoryWithDifferentRandomExt.cs")]
        [TestCase("ClassAMissingSuffix.cs")]
        public void TestsWithClassSuffixDifferentToDefault(string testName)
        {
            // the default suffix is 'Tests' - we test that this can be overidden 
#if R7
            this.ExecuteWithinSettingsTransaction(
                (settingsStore =>
                     {
                         this.RunGuarded((() =>
                                              {
                                                  IContextBoundSettingsStore
                                                      settings = settingsStore.BindToContextTransient
                                                          (ContextRange.ManuallyRestrictWritesToOneContext
                                                               (((lifetime, contexts) => contexts.Empty)));

                                                  settings.SetValue<TestFileAnalysisSettings, string>(
                                                      s => s.TestClassSuffix, "RandomExt");

                                              }));
                         DoTestFiles(testName);
                     }));
#else
            this.ExecuteWithinSettingsTransaction((Action<IContextBoundSettingsStore>)(settingsStore =>
            {
                RunGuarded((() => settingsStore.SetValue<TestFileAnalysisSettings, string>(s => s.TestClassSuffix, "RandomExt")));
                DoTestFiles(testName);
            }));
        
        
#endif
        }
    }
}
