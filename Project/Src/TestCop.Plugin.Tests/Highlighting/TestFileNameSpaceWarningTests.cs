// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System.IO;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.UI.ActionsRevised;
using NUnit.Framework;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.Highlighting
{    
    [TestFixture]
    public class TestFileNameSpaceWarningTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IPsiSourceFile sourceFile)
        {
            return highlighting is TestFileNameSpaceWarning;
        }

        protected override string RelativeTestDataPath
        {
            get { return @"highlighting\sample_sln"; }
        }

        protected override IExecutableAction GetShortcutAction(TextWriter textwriter)
        {
            var jumpToTestFileAction = JumpToTestFileAction.CreateWith(CreateJetPopMenuShowToWriterAction(textwriter));
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
