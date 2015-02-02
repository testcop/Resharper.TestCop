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
    public class IsARegexValidationRuleTests
    {
        const string ErrorMessage = "It is invalid";

        [Test]
        public void ValidatesValidRegexTest()
        {
            var validator = new IsARegexValidationRule();
            validator.MinimumGroupsInRegex = 0;
            
            var validationResult = validator.Validate("[A-Z]", CultureInfo.CurrentCulture);
            Assert.IsTrue(validationResult.IsValid);
        }

        [Test]
        public void InValidatesIncorrectRegexTest()
        {
            var validator = new IsARegexValidationRule();
            validator.ErrorMessage = ErrorMessage;
            validator.MinimumGroupsInRegex = 0;

            var validationResult = validator.Validate("[A-Z", CultureInfo.CurrentCulture);
            Assert.IsFalse(validationResult.IsValid);
            Assert.AreEqual(ErrorMessage, validationResult.ErrorContent.ToString());
        }

        [Test]
        public void InValidatesCorrectRegexButWithLessThanMinumumGroupsTest()
        {
            var validator = new IsARegexValidationRule();
            validator.ErrorMessage = ErrorMessage;
            
            validator.MinimumGroupsInRegex = 2;
            var validationResult = validator.Validate("(.*)-(.*)", CultureInfo.CurrentCulture);            
            Assert.IsTrue(validationResult.IsValid);

            validator.MinimumGroupsInRegex = 30;
            validationResult = validator.Validate("(.*)-(.*)", CultureInfo.CurrentCulture);            
            Assert.IsFalse(validationResult.IsValid);            
        }

    }
}
