using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp;
using JetBrains.Application.Settings;
using NUnit.Framework;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.Highlighting
{
    [TestFixture]
    public class ShouldBePublicWarningTests : CSharpHighlightingTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        {
            return highlighting is ShouldBePublicWarning;
        }

        protected override string RelativeTestDataPath
        {
            get { return @"highlighting\ShouldBePublicWarning"; }
        }

        [Test]
        [TestCase("PrivateNUnitTestClass.cs")]
        [TestCase("PrivateMSTestTestMethod.cs")]
        [TestCase("PrivateNUnitTestMethod.cs")]
        [TestCase("PrivateXUnitTestMethod.cs")]
        public void Test(string testName)
        {
            DoTestFiles(testName);
        }
    }
}
