// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Util;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper.Mapper
{
    public class TestProjectsMapToSingleCodeProjectHelper : MappingBase
    {                
        public override IList<TestCopProjectItem> GetAssociatedProject(IProject currentProject, string currentTypeNamespace)
        {
            const string warningMessage = "Not Supported: More than one code project has a default namespace of ";
            string subNameSpace = currentTypeNamespace.RemoveLeading(currentProject.GetDefaultNamespace());

            if (currentProject.IsTestProject())
            {
                var nameSpaceOfAssociateProject = GetNameSpaceOfAssociatedCodeProject(currentProject);

                var matchedCodeProjects = currentProject.GetSolution().GetNonTestProjects().Where(
                    p => p.GetDefaultNamespace()  == nameSpaceOfAssociateProject).ToList();

                if (matchedCodeProjects.Count() > 1)
                {
                    ResharperHelper.AppendLineToOutputWindow(warningMessage + nameSpaceOfAssociateProject);
                }

                return matchedCodeProjects.Select(p => new TestCopProjectItem(p, subNameSpace)).ToList();
            }

            var matchedTestProjects = currentProject.GetSolution().GetTestProjects().Where(
                p => GetNameSpaceOfAssociatedCodeProject(p) == currentProject.GetDefaultNamespace()).ToList();

            return matchedTestProjects.Select(p => new TestCopProjectItem(p, subNameSpace)).ToList();                                        
        }
       
        private static string GetNameSpaceOfAssociatedCodeProject(IProject testProject)
        {
            var testNameSpacePattern = Settings.TestProjectToCodeProjectNameSpaceRegEx;
            string replaceText = Settings.TestProjectToCodeProjectNameSpaceRegExReplace;

            string currentProjectNamespace = testProject.GetDefaultNamespace();
            if (string.IsNullOrEmpty(currentProjectNamespace)) return "";

            string result;
            if (RegexReplace(testNameSpacePattern, replaceText, currentProjectNamespace, out result)) return result;

            ResharperHelper.AppendLineToOutputWindow("ERROR: Regex pattern matching failed to extract group - check your regex replace string of " + replaceText);
            throw new ApplicationException("Unexpected internal error -regex error in testcop - {0} - {1}".FormatEx(testNameSpacePattern, replaceText));
        }      
    }
}