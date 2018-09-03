// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2017
// --

using System.Collections.Generic;
using JetBrains.ProjectModel;
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
                     
        public static IList<TestCopProjectItem> GetAssociatedProjects(this IProject currentProject, IProjectFile  projectFile, string overrideClassName=null)
        {            
            return ProjectMappingHelper.GetProjectMappingHeper().GetAssociatedProjectFor(currentProject, projectFile, overrideClassName);
        }              
    }
}
