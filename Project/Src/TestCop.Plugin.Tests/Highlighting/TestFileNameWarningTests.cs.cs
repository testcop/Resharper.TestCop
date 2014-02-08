// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System.IO;
using JetBrains.ActionManagement;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using NUnit.Framework;

namespace TestCop.Plugin.Tests.Highlighting
{
    [TestFixture]
    public class TestFileNameWarningTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        {
            return highlighting.GetType().FullName.Contains("TestCop");
        }

        protected override string RelativeTestDataPath
        {
            get { return @"highlighting\sample_sln"; }
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
