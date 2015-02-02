// // --
// // -- TestCop http://testcop.codeplex.com
// // -- License http://testcop.codeplex.com/license
// // -- Copyright 2013
// // --

using System.Globalization;
using NUnit.Framework;
using TestCop.Plugin.OptionsPage;

namespace TestCop.Plugin.Tests.OptionsPage
{
    [TestFixture]
    public class RegexValidationRuleTests
    {
        const string ErrorMessage = "It is invalid";

        [Test]
        public void ValidatesTextMatchingRegexTest()
        {
            var validator = new RegexValidationRule();
            validator.RegexText = ".B.";
            
            var validationResult = validator.Validate("ABC", CultureInfo.CurrentCulture);
            Assert.IsTrue(validationResult.IsValid);            
        }

        [Test]
        public void InValidatesTextNotMatchingRegexTest()
        {
            var validator = new RegexValidationRule();
            validator.RegexText = ".B.";            
            validator.ErrorMessage = ErrorMessage;

            var validationResult = validator.Validate("AXC", CultureInfo.CurrentCulture);
            Assert.IsFalse(validationResult.IsValid);
            Assert.AreEqual(ErrorMessage,validationResult.ErrorContent.ToString());            
        }
         
    }
}

