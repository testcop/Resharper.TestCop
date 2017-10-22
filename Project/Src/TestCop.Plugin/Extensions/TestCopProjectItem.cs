// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2017
// --

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Properties;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;

namespace TestCop.Plugin.Extensions
{
    public class TestCopProjectItem 
    {
        public struct FilePatternMatcher
        {
            public FilePatternMatcher(Regex regEx, string suffix)
            {
                Suffix = suffix;
                RegEx = regEx;
            }

            public string Suffix { get; }
            public Regex RegEx { get; }
        }

        private readonly IList<Tuple<string, bool>> _subDirectoryElements;
        
        /// <summary>
        /// Regex Patterns for files at this location
        /// </summary>
        public FilePatternMatcher[] FilePattern { get; set; }

        public enum ProjectItemTypeEnum {Tests, Code}

        /// <summary>
        /// location should contain these file types
        /// </summary>
        public ProjectItemTypeEnum ProjectItemType { get; }

        /// <summary>
        /// Parent Project
        /// </summary>
        public IProject Project { get; }

        /// <summary>
        /// Subnamespace of the parent project default namespace
        /// </summary>
        private string SubNamespace { get; }

        
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
                return FileSystemPath.Parse(Project.ProjectFileLocation.Directory + "\\" + _subDirectoryElements.Select(i => i.Item1).Join(@"\"));                
            }
        }

        public TestCopProjectItem(IProject project, ProjectItemTypeEnum projectItemType, string subNameSpace, IList<Tuple<string, bool>> subDirectoryElements, IEnumerable<FilePatternMatcher> filePatterns)
        {
            FilePattern = filePatterns.ToArray();
            Project = project;            
            SubNamespace = subNameSpace.RemoveLeading(".");

            var subNameSpaceAccordingToDirectoryElements = subDirectoryElements.Where(i => i.Item2).Select(i => i.Item1).Join(@".");
            Trace.Assert(subNameSpaceAccordingToDirectoryElements == SubNamespace);///TODO: remove the subnamespace parameter from constructor

            this._subDirectoryElements = subDirectoryElements;
            ProjectItemType = projectItemType;
        }

        public static IList<Tuple<string, bool>> ExtractFolders(IProjectItem item)
        {
            return ExtractFolders(item.ParentFolder);
        }

        public static IList<Tuple<string,bool>> ExtractFolders(IProjectFolder currentFolder)
        {
            IList<Tuple<string, bool>> foldersList = new List<Tuple<string, bool>>();

            var namespaceFolderProperty = currentFolder.GetSolution().GetComponent<NamespaceFolderProperty>();

            while (currentFolder != null) 
            {                              
                foldersList.Insert(0,new Tuple<string, bool>(currentFolder.Name,namespaceFolderProperty.GetNamespaceFolderProperty(currentFolder)));                                    
                currentFolder = currentFolder.ParentFolder;
            }

            if(foldersList.Count>0)foldersList.RemoveAt(0);//we don't want the parent folder

            return foldersList;
        }
    }
}