// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System;
using System.IO;
using JetBrains.ActionManagement;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
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

        protected override IActionHandler GetShortcutAction(TextWriter textwriter)
        {
            IActionHandler jumpToTestFileAction = new JumpToTestFileAction(CreateJetPopMenuShowToWriterAction(textwriter));
            return jumpToTestFileAction;
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
            string altRegEx = "^(.*)Tests$";
            // the default namespace is '^(.*)\.Tests$' - we test that this can be overidden with '^(.*)Tests$'            
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
                        s => s.TestProjectToCodeProjectNameSpaceRegEx, altRegEx);

                }));
                DoTestFiles(testName);
            }));
#else   
            this.ExecuteWithinSettingsTransaction((Action<IContextBoundSettingsStore>)(settingsStore =>
            {
                this.RunGuarded((Action)(() => settingsStore.SetValue<TestFileAnalysisSettings, string>(s => s.TestProjectToCodeProjectNameSpaceRegEx, altRegEx)));
                DoTestFiles(testName);
            }));
#endif
        }
       
    }
}
