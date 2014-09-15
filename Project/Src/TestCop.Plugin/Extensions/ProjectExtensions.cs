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
        /// <summary>
        /// project namespace matches TestCop regex
        /// </summary>        
        public static bool IsTestProject(this IProject project)
        {
            return ProjectMappingHelper.GetProjectMappingHeper().IsTestProject(project);          
        }
                     
        public static IList<TestCopProjectItem> GetAssociatedProjects(this IProject currentProject, string currentNamespace)
        {
            return ProjectMappingHelper.GetProjectMappingHeper().GetAssociatedProject(currentProject, currentNamespace);
        }       
    }
}
