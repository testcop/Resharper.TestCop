// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.Util;
using TestCop.Plugin.Helper;

namespace TestCop.Plugin.Extensions
{
    public static class ProjectExtensions
    {
        /// <summary>
        /// project namespace matches TestCop regex
        /// </summary>        
        public static bool IsTestProject(this IProject project)
        {            
            string currentProjectNamespace = project.GetDefaultNamespace();
            if (string.IsNullOrEmpty(currentProjectNamespace)) return false;

            return TestingRegEx.IsMatch(currentProjectNamespace);
        }
     
        private static string GetNameSpaceOfAssociateProject(this IProject project)
        {
            string replaceText=TestCopSettingsManager.Instance.Settings.TestProjectToCodeProjectNameSpaceRegExReplace;

            string currentProjectNamespace = project.GetDefaultNamespace();
            if (string.IsNullOrEmpty(currentProjectNamespace)) return "";

            var match = TestingRegEx.Match(currentProjectNamespace);
            if (match.Success && match.Groups.Count>1)
            {
                if (replaceText.IsNullOrEmpty() || replaceText == "*")
                {
                    string result = "";
                    for (int i = 1; i < match.Groups.Count; i++) result += match.Groups[i].Value;
                    return result;
                }

                return TestingRegEx.Replace(currentProjectNamespace, replaceText);                
            }

            ResharperHelper.AppendLineToOutputWindow("ERROR: Regex pattern matching failed to extract group");
            throw new ApplicationException("Unexpected internal error.");
        }

        private static Regex TestingRegEx
        {
            get
            {
                var testNameSpacePattern = TestCopSettingsManager.Instance.Settings.TestProjectToCodeProjectNameSpaceRegEx;
                var regEx = new Regex(testNameSpacePattern);
                return regEx;
            }
        }

        public static IList<IProject> GetAssociatedProject(this IProject currentProject)
        {
            const string warningMessage = "Not Supported: More than one code project has a default namespace of ";

            if (currentProject.IsTestProject())
            {
                var nameSpaceOfAssociateProject = currentProject.GetNameSpaceOfAssociateProject();

                var matchedCodeProjects=currentProject.GetSolution().GetNonTestProjects().Where(
                    p => p.GetDefaultNamespace() == nameSpaceOfAssociateProject).ToList();

                if (matchedCodeProjects.Count() > 1)
                {
                    ResharperHelper.AppendLineToOutputWindow(warningMessage + nameSpaceOfAssociateProject);
                    return matchedCodeProjects; 
                }

                return matchedCodeProjects;
            }

            var matchedTestProjects = currentProject.GetSolution().GetTestProjects().Where(
                p => p.GetNameSpaceOfAssociateProject() == currentProject.GetDefaultNamespace()).ToList();
           
            return matchedTestProjects;                                        
        }
    }
}
