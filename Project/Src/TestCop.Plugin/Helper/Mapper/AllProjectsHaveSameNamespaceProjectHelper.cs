// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Util;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper.Mapper
{
    public class AllProjectsHaveSameNamespaceProjectHelper: MappingBase
    {
        public override bool IsTestProject(IProject project)
        {
            string currentProjectName = project.Name;
            if (string.IsNullOrEmpty(currentProjectName)) return false;

            return TestingRegEx.IsMatch(currentProjectName);
        }

        public override IList<TestCopProjectItem> GetAssociatedProject(IProject currentProject, string currentTypeNamespace)
        {
            const string warningMessage = "Not Supported: More than one  project has a name of ";

            string subNameSpace = currentTypeNamespace.RemoveLeading(currentProject.GetDefaultNamespace());

            if (currentProject.IsTestProject())
            {
                var nameOfAssociateProject = GetNameOfAssociateCodeProject(currentProject);

                var matchedCodeProjects = currentProject.GetSolution().GetNonTestProjects().Where(
                    p => p.Name == nameOfAssociateProject).ToList();

                var badProjects=matchedCodeProjects.Where(p => p.GetDefaultNamespace() != currentProject.GetDefaultNamespace()).ToList();
                matchedCodeProjects.RemoveAll(badProjects.Contains);

                badProjects.ForEach(p =>ResharperHelper.AppendLineToOutputWindow("Project {0} should have namespace of {1}".FormatEx(p.Name, currentProject.GetDefaultNamespace())));

                if (matchedCodeProjects.Count() > 1)
                {
                    ResharperHelper.AppendLineToOutputWindow(warningMessage + nameOfAssociateProject);
                }

                return matchedCodeProjects.Select(p => new TestCopProjectItem(p, subNameSpace)).ToList();
            }

            var matchedTestProjects = currentProject.GetSolution().GetTestProjects().Where(
                p => GetNameOfAssociateCodeProject(p) == currentProject.Name).ToList();

            var badTestProjects=matchedTestProjects.Where(p => p.GetDefaultNamespace() != currentProject.GetDefaultNamespace()).ToList();
            matchedTestProjects.RemoveAll(badTestProjects.Contains);
            badTestProjects.ForEach(p => ResharperHelper.AppendLineToOutputWindow("Project {0} should have namespace of {1}".FormatEx(p.Name, currentProject.GetDefaultNamespace())));
            
            return matchedTestProjects.Select(p => new TestCopProjectItem(p, subNameSpace)).ToList();                                        
        }

        private static string GetNameOfAssociateCodeProject(IProject testProject)
        {            
            var testNamePattern = Settings.TestProjectToCodeProjectNameSpaceRegEx;
            string replaceText = Settings.TestProjectToCodeProjectNameSpaceRegExReplace;

            string currentProjectName = testProject.Name;
            if (string.IsNullOrEmpty(currentProjectName)) return "";

            string result;
            if (RegexReplace(testNamePattern, replaceText, currentProjectName, out result)) return result;

            ResharperHelper.AppendLineToOutputWindow("ERROR: Regex pattern matching failed to extract group");
            throw new ApplicationException("Unexpected internal error.");
        }      
    }
}