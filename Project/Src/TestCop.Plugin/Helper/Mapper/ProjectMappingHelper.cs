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
            var settings = TestCopSettingsManager.Instance.Settings;
            
            switch (settings.TestCopProjectStrategy)
            {
                case TestProjectStrategy.SingleTestProjectPerSolution:
                    return new CodeProjectMapsToSingleTestProjectHeper();
                case TestProjectStrategy.TestProjectHasSameNamespaceAsCodeProject:
                    return new AllProjectsHaveSameNamespaceProjectHelper();
                
                default:
                    return new TestProjectsMapToSingleCodeProjectHelper();
            }          
        } 
    }
}