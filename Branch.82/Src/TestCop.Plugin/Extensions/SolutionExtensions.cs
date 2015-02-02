// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System.Collections.Generic;
using System.Linq;
using JetBrains.ProjectModel;

namespace TestCop.Plugin.Extensions
{
    public static class SolutionExtensions
    {
        /// <summary>
        /// Returns all test projects within solution
        /// </summary>        
        public static IEnumerable<IProject> GetTestProjects(this ISolution solution)
        {            
            return GetAllCodeProjects(solution).Where(x => x.IsTestProject());
        }

        /// <summary>
        /// Returns all 'real' user projects within solution
        /// </summary>
        public static IEnumerable<IProject> GetAllCodeProjects(this ISolution solution)
        {
            return solution.GetAllProjects().Where(p => p.IsProjectFromUserView());
        }

        /// <summary>
        /// Returns all non test projects within solution
        /// </summary>  
        public static IEnumerable<IProject> GetNonTestProjects(this ISolution solution)
        {
            return GetAllCodeProjects(solution).Where(x => !x.IsTestProject());
        }
    }
}
