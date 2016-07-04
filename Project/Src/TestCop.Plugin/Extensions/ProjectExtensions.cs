// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
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
                     
        public static IList<TestCopProjectItem> GetAssociatedProjects(this IProject currentProject, IProjectFile  projectFile)
        {
            string currentNamespace = projectFile.CalculateExpectedNamespace(projectFile.GetPrimaryPsiFile().Language);
            return ProjectMappingHelper.GetProjectMappingHeper().GetAssociatedProject(currentProject, projectFile.Location.NameWithoutExtension, currentNamespace);
        }

        public static IList<TestCopProjectItem> GetAssociatedProjects(this IProject currentProject, ITypeElement classInProject)
        {
            string currentNamespace = classInProject.OwnerNamespaceDeclaration();
            return ProjectMappingHelper.GetProjectMappingHeper().GetAssociatedProject(currentProject, classInProject.ShortName, currentNamespace);
            //string currentNamespace = projectFile.CalculateExpectedNamespace(projectFile.GetPrimaryPsiFile().Language);
            //return ProjectMappingHelper.GetProjectMappingHeper().GetAssociatedProject(currentProject, projectFile.Location.NameWithoutExtension, currentNamespace);
        } 
    }
}
