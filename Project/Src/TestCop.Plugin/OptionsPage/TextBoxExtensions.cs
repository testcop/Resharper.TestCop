// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2015
// --
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace TestCop.Plugin.OptionsPage
{
    static class TextBoxExtensions
    {
        public static void BindWithRegexMatchesValidation(this TextBox tb, TestFileAnalysisSettings testFileAnalysisSettings, string property, string regexString, string errorMessage = "Invalid suffix.")
        {
            var binding = new Binding { Path = new PropertyPath(property) };
            var namespaceRule = new RegexValidationRule
            {
                RegexText = regexString,
                ErrorMessage = errorMessage,
                RegexOptions = RegexOptions.IgnoreCase,
                ValidatesOnTargetUpdated = true
            };

            binding.ValidationRules.Add(namespaceRule);
            binding.NotifyOnValidationError = true;
            binding.ValidatesOnDataErrors = true;
            binding.ValidatesOnExceptions = true;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            tb.DataContext = testFileAnalysisSettings;
            tb.SetBinding(TextBox.TextProperty, binding);
        }

        public static void BindWithValidationMustBeARegex(this TextBox tb, TestFileAnalysisSettings testFileAnalysisSettings, string property)
        {
            var binding = new Binding { Path = new PropertyPath(property) };
            var namespaceRule = new IsARegexValidationRule
            {
                ErrorMessage = "Invalid Regex",
                RegexOptions = RegexOptions.IgnoreCase,
                MinimumGroupsInRegex = 2,
                ValidatesOnTargetUpdated = true
            };

            binding.ValidationRules.Add(namespaceRule);
            binding.NotifyOnValidationError = true;
            binding.ValidatesOnDataErrors = true;
            binding.ValidatesOnExceptions = true;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            tb.DataContext = testFileAnalysisSettings;
            tb.SetBinding(TextBox.TextProperty, binding);
        }
    }
}
