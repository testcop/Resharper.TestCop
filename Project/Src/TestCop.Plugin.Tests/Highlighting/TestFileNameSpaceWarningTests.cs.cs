using JetBrains.ReSharper.Daemon;
using JetBrains.Application.Settings;
using NUnit.Framework;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.Highlighting
{
    //TODO: Review TestGotoFile
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
