using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace TestCop.Plugin.OptionsPage
{
    public class RegexValidationRule : ValidationRule
    {
        public string RegexText { get; set; }
        public string ErrorMessage { get; set; }
        public RegexOptions RegexOptions { get; set; }

        public override ValidationResult Validate(object value,
                                                  CultureInfo cultureInfo)
        {
            ValidationResult result = ValidationResult.ValidResult;
           
            if (!String.IsNullOrEmpty(this.RegexText))
            {              
                string text = value as string ?? String.Empty;

               
                if (!Regex.IsMatch(text, this.RegexText, this.RegexOptions))
                    result = new ValidationResult(false, this.ErrorMessage);
            }
            
            return result;
        }
    }
}