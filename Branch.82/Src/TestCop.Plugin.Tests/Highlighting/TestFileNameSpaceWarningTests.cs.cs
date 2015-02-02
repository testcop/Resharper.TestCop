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
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.Highlighting
{    
    [TestFixture]
    public class TestFileNameSpaceWarningTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        {
            return highlighting is TestFileNameSpaceWarning;
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
        [TestCase(@"<TestApplication.Tests>\Samples\ClassCTests.cs")]
        [TestCase(@"<TestApplication.Tests>\ClassDTests.cs")]     
        public void Test(string testName)
        {
            DoTestFiles(testName);
        }
       
    }
}
