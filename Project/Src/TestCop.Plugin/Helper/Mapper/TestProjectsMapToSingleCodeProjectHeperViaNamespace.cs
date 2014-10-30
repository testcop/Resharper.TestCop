// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Util;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper.Mapper
{
    public class TestProjectsMapToSingleCodeProjectHeperViaNamespace : MappingBase
    {
        public override IList<TestCopProjectItem> GetAssociatedProject(IProject currentProject, string currentTypeNamespace)
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
            var testNameSpacePattern = Settings.TestProjectToCodeProjectNameSpaceRegEx;
            string replaceText = Settings.TestProjectToCodeProjectNameSpaceRegExReplace;

            string currentProjectNamespace = project.GetDefaultNamespace();
            if (string.IsNullOrEmpty(currentProjectNamespace)) return "";

            string result;
            if (RegexReplace(testNameSpacePattern, replaceText, currentProjectNamespace, out result)) return result;

            ResharperHelper.AppendLineToOutputWindow("ERROR: Regex pattern matching failed to extract group");
            throw new ApplicationException("Unexpected internal error.");
        }      
    }
}