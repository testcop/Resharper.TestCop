// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Util;
using TestCop.Plugin.Helper.Mapper;

namespace TestCop.Plugin.Extensions
{
    public static class ProjectExtensions
    {
        private static readonly IProjectMappingHeper MappingHelper = ProjectMappingHelper.GetProjectMappingHeper();

        /// <summary>
        /// project namespace matches TestCop regex
        /// </summary>        
        public static bool IsTestProject(this IProject project)
        {            
            string currentProjectNamespace = project.GetDefaultNamespace();
            if (string.IsNullOrEmpty(currentProjectNamespace)) return false;

            return TestingRegEx.IsMatch(currentProjectNamespace);
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
        
        public static IList<TestCopProjectItem> GetAssociatedProjects(this IProject currentProject, string currentNamespace)
        {
            return MappingHelper.GetAssociatedProject(currentProject, currentNamespace);
        }       
    }
}
