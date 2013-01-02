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
        public void Test(string testName)
        {
            DoTestFiles(testName);
        }
       
    }
}
