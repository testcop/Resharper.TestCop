// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace TestCop.Plugin.OptionsPage
{
    public class IsARegexValidationRule : ValidationRule
    {        
        public string ErrorMessage { get; set; }
        public RegexOptions RegexOptions { get; set; }
        public int MinimumGroupsInRegex { get; set; }

        public override ValidationResult Validate(object value,
            CultureInfo cultureInfo)
        {
            ValidationResult result = ValidationResult.ValidResult;           
            string text = value as string ?? String.Empty;

            try
            {                
                var rx = new Regex(text);
                if (rx.GetGroupNames().Length < MinimumGroupsInRegex)
                {
                    result = new ValidationResult(false,
                        string.Format("RegEx must contain at least {0} regex group ().", MinimumGroupsInRegex - 1) );
                }
            }
            catch (Exception)
            {
                result = new ValidationResult(false, ErrorMessage); 
            }                
            
            return result;
        }
    }
}