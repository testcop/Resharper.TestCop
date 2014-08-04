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
    public class CodeProjectMapsToSingleTestProjectHeper : IProjectMappingHeper
    {        
        // http://myregexp.com/
        public IList<TestCopProjectItem> GetAssociatedProject(IProject currentProject, string currentTypeNamespace)
        {
            var settings = TestCopSettingsManager.Instance.Settings;
            const string warningMessage = "Not Supported: More than one code project has a default namespace of ";
          
            if (currentProject.IsTestProject())
            {
                string subNameSpace = currentTypeNamespace.RemoveLeading(currentProject.GetDefaultNamespace());
                // <MyCorp.App.Tests>.API.ClassA --> <MyCorp.App.API>.ClassA

                string nameSpaceOfAssociateProject;
                TestProjectsMapToSingCodeProjectHeper.RegexReplace(settings.SingleTestRegexTestToAssembly
                    , settings.SingleTestRegexTestToAssemblyProjectReplace, currentTypeNamespace,
                    out nameSpaceOfAssociateProject);

                TestProjectsMapToSingCodeProjectHeper.RegexReplace(settings.SingleTestRegexTestToAssembly
                    , settings.SingleTestRegexTestToAssemblyProjectSubNamespaceReplace, currentTypeNamespace,
                     out subNameSpace);

                var matchedCodeProjects = currentProject.GetSolution().GetNonTestProjects().Where(
                    p => p.GetDefaultNamespace()  == nameSpaceOfAssociateProject).ToList();

                if (matchedCodeProjects.Count() > 1)
                {
                    ResharperHelper.AppendLineToOutputWindow(warningMessage + nameSpaceOfAssociateProject);
                }

                return matchedCodeProjects.Select(p => new TestCopProjectItem(p, subNameSpace)).ToList();
            }

            string subNameSpaceOfTest;
            TestProjectsMapToSingCodeProjectHeper.RegexReplace(settings.SingleTestRegexCodeToTestAssembly
                , settings.SingleTestRegexCodeToTestReplace, currentTypeNamespace,
                out subNameSpaceOfTest);
            
            var matchedTestProjects = currentProject.GetSolution().GetTestProjects().ToList();
            if (matchedTestProjects.Count > 1)
            {
                ResharperHelper.AppendLineToOutputWindow("Not Supported: Expected only one test project for all code projects to use");                
            }

            return matchedTestProjects.Select(p => new TestCopProjectItem(p, subNameSpaceOfTest)).Take(1).ToList();                                        
        }
    }
}