// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using JetBrains.Application.Components;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp;
using JetBrains.TestFramework.ProjectModel;
using NUnit.Framework;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.Highlighting
{
    [TestFixture]
    public class ShouldBePublicWarningTests : CSharpHighlightingTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        {
            return highlighting is AbstractShouldBePublicWarning;
        }

        protected override string RelativeTestDataPath
        {
            get { return @"highlighting\ShouldBePublicWarning"; }
        }

        #if !R7
        public override void TestFixtureTearDown()
        {
            /* this logic is needed to undo the Resharper solution caching in place for tests
             * that break the more complex 'solution based tests' within the test solution */
            base.TestFixtureTearDown();

            RunGuarded(
              () => ShellInstance.GetComponent<ReuseSolutionInTestsComponent>().CloseSolution());
        }
        #endif

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
