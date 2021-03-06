// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2013
// --

using System.IO;
using JetBrains.Application.Settings;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using NUnit.Framework;

namespace TestCop.Plugin.Tests.Highlighting
{
    [TestFixture]
    public class TestFileNameWarningTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile, IContextBoundSettingsStore settingsStore)
        {
            return highlighting.GetType().FullName.Contains("TestCop");
        }

        protected override string RelativeTestDataPath
        {
            get { return @"highlighting\sample_sln"; }
        }

        protected override IExecutableAction GetShortcutAction(TextWriter textwriter)
        {
            var jumpToTestFileAction = TestCopJumpToTestFileAction.CreateWith(CreateJetPopMenuShowToWriterAction(textwriter));
            return jumpToTestFileAction;
        }

        protected override string SolutionName
        {
            get { return @"TestApplication.sln"; }
        }

        [Test]
        [TestCase(@"<TestApplication.Tests>\Samples\ClassB.SecurityTests.cs")]
        [TestCase(@"<TestApplication.Tests>\ClassATests.cs")]
        [TestCase(@"<TestApplication.Tests>\ClassA.SomeMoreTests.cs")]
        [TestCase(@"<TestApplication.Tests>\ClassE.WithNestedTests.cs")]
        [TestCase(@"<TestApplication.Tests>\AbstractTestClass.cs")]  
        public void Test(string testName)
        {
            DoTestFiles(testName);
        }
       
    }
}
