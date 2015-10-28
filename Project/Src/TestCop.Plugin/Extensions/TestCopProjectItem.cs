// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2015
// --

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;

namespace TestCop.Plugin.Extensions
{
    public class TestCopProjectItem 
    {   
        /// <summary>
        /// Regex Patterns for files at this location
        /// </summary>
        public Regex[] FilePattern { get; set; }

        public enum ProjectItemTypeEnum {Tests, Code}

        /// <summary>
        /// location should contain these file types
        /// </summary>
        public ProjectItemTypeEnum ProjectItemType { get; private set; }

        /// <summary>
        /// Parent Project
        /// </summary>
        public IProject Project { get; private set; }

        /// <summary>
        /// Subnamespace of the parent project default namespace
        /// </summary>
        private string SubNamespace { get; set; }

        
        /// <summary>
        /// Fullnamespace expected for this item (if it exists)
        /// </summary>        
        public string FullNamespace()
        {
            if (String.IsNullOrEmpty(SubNamespace))
            {
                return Project.GetDefaultNamespace();
            }
            return Project.GetDefaultNamespace().AppendIfMissing(".") + SubNamespace;
        }

        /// <summary>
        /// Filesystem folder expected for this item (if it exists)
        /// </summary>        
        public FileSystemPath SubNamespaceFolder
        {
            get
            {                
                return FileSystemPath.Parse(Project.ProjectFileLocation.Directory + "\\" + SubNamespace.Replace('.', '\\'));
            }
        }

        public TestCopProjectItem(IProject project, ProjectItemTypeEnum projectItemType, string subNameSpace, IEnumerable<string> filePatterns)
        {
            FilePattern = filePatterns.Select(f=>new Regex(f)).ToArray();
            Project = project;            
            SubNamespace = subNameSpace.RemoveLeading(".");
            ProjectItemType = projectItemType;
        }           
    }
}