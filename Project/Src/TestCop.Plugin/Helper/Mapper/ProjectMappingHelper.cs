// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

namespace TestCop.Plugin.Helper.Mapper
{
    public static class ProjectMappingHelper
    {
        public static IProjectMappingHeper GetProjectMappingHeper()
        {
            return new TestProjectsMapToSingCodeProjectHeper();
        } 
    }
}