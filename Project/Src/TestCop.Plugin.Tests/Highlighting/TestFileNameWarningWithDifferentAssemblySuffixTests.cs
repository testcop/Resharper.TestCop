using System;
using JetBrains.ReSharper.Daemon;
using JetBrains.Application.Settings;
using NUnit.Framework;

namespace TestCop.Plugin.Tests.Highlighting
{
    [TestFixture]
    public class TestFileNameWarningWithDifferentAssemblySuffixTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        {
            return highlighting.GetType().FullName.Contains("TestCop");
        }

        protected override string RelativeTestDataPath
        {
            get { return @"highlighting\sample_sln_2"; }
        }

        protected override string SolutionName
        {
            get { return @"TestApplication.sln"; }
        }

        [Test]        		
        [TestCase(@"<TestApplication2Tests>\Samples\ClassB.SecurityTests.cs")]
        [TestCase(@"<TestApplication2Tests>\ClassATests.cs")]        
        [TestCase(@"<TestApplication2Tests>\ClassA.SomeMoreTests.cs")]       
        public void Test(string testName)
        {
            // the default namespace is '^(.*)\.Tests$' - we test that this can be overidden with '^(.*)Tests$'            
#if R7
        this.ExecuteWithinSettingsTransaction(
            (settingsStore =>
            {
                this.RunGuarded((() =>
                {
                    settingsStore.SetBinding();
                        
                    IContextBoundSettingsStore
                        settings = settingsStore.BindToContextTransient
                                    (ContextRange.ManuallyRestrictWritesToOneContext
                                        (((lifetime, contexts) => contexts.Empty)));
                        
                    settings.SetValue<TestFileAnalysisSettings, string>(
                        s => s.TestNameSpaceSuffix, "Tests");

                }));
                DoTestFiles(testName);
            }));
#else   
            this.ExecuteWithinSettingsTransaction((Action<IContextBoundSettingsStore>)(settingsStore =>
            {
                this.RunGuarded((Action)(() => settingsStore.SetValue<TestFileAnalysisSettings, string>(s => s.TestProjectToCodeProjectNameSpaceRegEx, "^(.*)Tests$")));
                DoTestFiles(testName);
            }));
#endif
        }
       
    }
}
