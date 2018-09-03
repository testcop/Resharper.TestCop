// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2016
// --

using System.Collections.Generic;
using JetBrains.ProjectModel;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper.Mapper
{
    public interface IProjectMappingHeper
    {
        IList<TestCopProjectItem> GetAssociatedProjectFor(IProject currentProject, IProjectFile projectFile, string overrideClassName = null);
        bool IsTestProject(IProject project);
        void DumpDebug(ISolution solution);
    }
}