// // --
// // -- TestCop http://testcop.codeplex.com
// // -- License http://testcop.codeplex.com/license
// // -- Copyright 2014
// // --

using NUnit.Framework;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Tests.Extensions
{
    [TestFixture]
    public class TestCopSettingsExtensionsTests
    {
        [Test]
        public void TestClassSuffixesAreSortedLongestFirst()
        {
            var settings = new TestFileAnalysisSettings {TestClassSuffix = "Tests,IntegrationTests,Test"};

            Assert.AreEqual(3, settings.TestClassSuffixes().Count);
            Assert.AreEqual("IntegrationTests", settings.TestClassSuffixes()[0]);
            Assert.AreEqual("Tests", settings.TestClassSuffixes()[1]);
            Assert.AreEqual("Test", settings.TestClassSuffixes()[2]);
        }

        [Test]
        public void TestClassSuffixesHandlesSingleEntry()
        {
            var settings = new TestFileAnalysisSettings {TestClassSuffix = "Tests"};

            Assert.AreEqual(1, settings.TestClassSuffixes().Count);
            Assert.AreEqual("Tests", settings.TestClassSuffixes()[0]);
        }
    }
}