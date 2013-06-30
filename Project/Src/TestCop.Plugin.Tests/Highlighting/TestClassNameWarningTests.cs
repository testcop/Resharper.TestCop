using System;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp;
using JetBrains.Application.Settings;
using NUnit.Framework;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.Highlighting
{
    [TestFixture]
    public class TestClassNameWarningTests : CSharpHighlightingTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        {
            return highlighting is TestClassNameWarning;
        }

        protected override string RelativeTestDataPath
        {
            get { return @"highlighting\TestClassNameWarning"; }
        }

        [Test]
        [TestCase("ClassA.SomeCategoryTests.cs")]
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
                this.RunGuarded((Action)(() => settingsStore.SetValue<TestFileAnalysisSettings, string>(s => s.TestClassSuffix, "RandomExt")));
                DoTestFiles(testName);
            }));
        
        
#endif
        }
    }
}
