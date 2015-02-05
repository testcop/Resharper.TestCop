using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TestCop.Plugin.Tests.SingleTestProjectToMultipleCodeProject
{
    [TestFixture]
    public class RegExTests
    {
        public const string RegexForSingleTestProjectStrategy = @"^(.*)\.Tests(\..*?)?(\..*)*$";

        [Test]
        public void CheckRegexIsCorrectForSingleTestProjectStrategyTests()
        {
            const string nsOfProject = "MyCorp.TestApplication3.Tests";
            const string nsOfFile = "MyCorp.TestApplication3.Tests.DAL.NS1.NS2";

            Assert.IsTrue(new Regex(RegexForSingleTestProjectStrategy).IsMatch(nsOfProject), "NS not a match");
            Assert.IsTrue(new Regex(RegexForSingleTestProjectStrategy).IsMatch(nsOfFile), "FL not a match");
            var g = new Regex(RegexForSingleTestProjectStrategy).Match(nsOfFile).Groups;

            Assert.AreEqual("MyCorp.TestApplication3", new Regex(RegexForSingleTestProjectStrategy).Match(nsOfFile).Groups[1].Value);
            Assert.AreEqual(".DAL", new Regex(RegexForSingleTestProjectStrategy).Match(nsOfFile).Groups[2].Value);
            Assert.AreEqual(".NS1.NS2", new Regex(RegexForSingleTestProjectStrategy).Match(nsOfFile).Groups[3].Value);
        }
    }
}
