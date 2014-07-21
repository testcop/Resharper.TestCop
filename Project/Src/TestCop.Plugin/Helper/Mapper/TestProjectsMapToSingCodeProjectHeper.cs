// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.Util;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper.Mapper
{
    public class TestProjectsMapToSingCodeProjectHeper : IProjectMappingHeper
    {
        public IList<TestCopProjectItem> GetAssociatedProject(IProject currentProject, string currentTypeNamespace)
        {
            const string warningMessage = "Not Supported: More than one code project has a default namespace of ";
            string subNameSpace = currentTypeNamespace.RemoveLeading(currentProject.GetDefaultNamespace());

            if (currentProject.IsTestProject())
            {
                var nameSpaceOfAssociateProject = GetNameSpaceOfAssociateProject(currentProject);

                var matchedCodeProjects = currentProject.GetSolution().GetNonTestProjects().Where(
                    p => p.GetDefaultNamespace()  == nameSpaceOfAssociateProject).ToList();

                if (matchedCodeProjects.Count() > 1)
                {
                    ResharperHelper.AppendLineToOutputWindow(warningMessage + nameSpaceOfAssociateProject);
                }

                return matchedCodeProjects.Select(p => new TestCopProjectItem(p, subNameSpace)).ToList();
            }

            var matchedTestProjects = currentProject.GetSolution().GetTestProjects().Where(
                p => GetNameSpaceOfAssociateProject(p) == currentProject.GetDefaultNamespace()).ToList();

            return matchedTestProjects.Select(p => new TestCopProjectItem(p, subNameSpace)).ToList();                                        
        }

        private static string GetNameSpaceOfAssociateProject(IProject project)
        {
            var testNameSpacePattern = TestCopSettingsManager.Instance.Settings.TestProjectToCodeProjectNameSpaceRegEx;
            string replaceText = TestCopSettingsManager.Instance.Settings.TestProjectToCodeProjectNameSpaceRegExReplace;

            string currentProjectNamespace = project.GetDefaultNamespace();
            if (string.IsNullOrEmpty(currentProjectNamespace)) return "";

            string result;
            if (RegexReplace(testNameSpacePattern, replaceText, currentProjectNamespace, out result)) return result;

            ResharperHelper.AppendLineToOutputWindow("ERROR: Regex pattern matching failed to extract group");
            throw new ApplicationException("Unexpected internal error.");
        }

        public static bool RegexReplace(string regexPattern, string regexReplaceText, string inputString, out string resultString )
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

                resultString= regex.Replace(inputString, regexReplaceText);
                return true;
            }
            return false;
        }      
    }
}