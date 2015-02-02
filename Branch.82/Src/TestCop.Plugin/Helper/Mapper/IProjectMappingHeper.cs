// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System.Collections.Generic;
using JetBrains.ProjectModel;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper.Mapper
{
    public interface IProjectMappingHeper
    {
        IList<TestCopProjectItem> GetAssociatedProject(IProject currentProject, string currentNameSpace);
        bool IsTestProject(IProject project);
        void DumpDebug(ISolution solution);
    }
}