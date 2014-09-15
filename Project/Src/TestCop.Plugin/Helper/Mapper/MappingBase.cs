using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.Util;

namespace TestCop.Plugin.Helper.Mapper
{
    public class MappingBase
    {
        public virtual bool IsTestProject(IProject project)
        {
            string currentProjectNamespace = project.GetDefaultNamespace();
            if (string.IsNullOrEmpty(currentProjectNamespace)) return false;

            return TestingRegEx.IsMatch(currentProjectNamespace);
        }

        protected static Regex TestingRegEx
        {
            get
            {
                var testNameSpacePattern = Settings.TestProjectToCodeProjectNameSpaceRegEx;
                var regEx = new Regex(testNameSpacePattern);
                return regEx;
            }
        }

        protected static TestFileAnalysisSettings Settings
        {
            get { return TestCopSettingsManager.Instance.Settings; }
        }

        public static bool RegexReplace(string regexPattern, string regexReplaceText, string inputString, out string resultString)
        {
            resultString = "";
            var regex = new Regex(regexPattern);
            var match = regex.Match(inputString);

            if (match.Success && match.Groups.Count > 1)
            {
                if (regexReplaceText.IsNullOrEmpty() || regexReplaceText == "*")
                {
                    for (int i = 1; i < match.Groups.Count; i++) resultString += match.Groups[i].Value;
                    return true;
                }

                resultString = regex.Replace(inputString, regexReplaceText);
                return true;
            }
            return false;
        } 
    }
}