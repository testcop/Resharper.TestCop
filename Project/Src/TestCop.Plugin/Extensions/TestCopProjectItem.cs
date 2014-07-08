// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using JetBrains.ProjectModel;
using JetBrains.Util;

namespace TestCop.Plugin.Extensions
{
    public class TestCopProjectItem
    {
        public IProject Project { get; private set; }
        public string SubNamespace { get; private set; }

        public FileSystemPath SubNamespaceFolder
        {
            get
            {
                return FileSystemPath.Parse(Project.ProjectFileLocation.Directory + "\\" + SubNamespace.Replace('.', '\\'));
            }
        }

        public TestCopProjectItem(IProject project, string subNameSpace)
        {
            Project = project;
            SubNamespace = subNameSpace;
        }        
    }
}