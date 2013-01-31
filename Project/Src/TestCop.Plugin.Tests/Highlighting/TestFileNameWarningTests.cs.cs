using JetBrains.ReSharper.Daemon;
using JetBrains.Application.Settings;
using NUnit.Framework;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.Highlighting
{
    [TestFixture]
    public class TestFileNameWarningTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        {
            return true;
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
        [TestCase(@"<TestApplication.Tests>\Samples\ClassB.SecurityTests.cs")]
        [TestCase(@"<TestApplication.Tests>\ClassATests.cs")]
        [TestCase(@"<TestApplication.Tests>\ClassA.SomeMoreTests.cs")]       
        public void Test(string testName)
        {
            DoTestFiles(testName);
        }
       
    }
}
