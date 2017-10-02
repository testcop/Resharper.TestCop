// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper
{    
    public class ProjectFileFinder: RecursiveProjectVisitor
    {
        private TestCopProjectItem.FilePatternMatcher[] _filePatterns;        
        private readonly List<Match> _itemMatches;

        public struct Match
        {
            public readonly TestCopProjectItem.FilePatternMatcher Matcher;

            public Match(IProjectFile projectFile, TestCopProjectItem.FilePatternMatcher matcher)
            {
                Matcher = matcher;                
                ProjectFile = projectFile;
            }

            public IProjectFile ProjectFile { get; }
        }


        public ProjectFileFinder(List<Match> itemMatches, params TestCopProjectItem.FilePatternMatcher[] filePatterns)
        {
            _itemMatches = itemMatches;            
            this._filePatterns = filePatterns;
        }

        public ProjectFileFinder(List<Match> items, Regex regex) : this(items, new TestCopProjectItem.FilePatternMatcher(regex,""))
        {
            
        }

        public override void VisitProjectFile(IProjectFile projectFile)
        {
            base.VisitProjectFile(projectFile);
            string projectFileName = projectFile.Location.NameWithoutExtension;

            if (projectFile.Kind == ProjectItemKind.PHYSICAL_FILE)
            {
                foreach (var pattern in _filePatterns)
                {
                    if (pattern.RegEx.IsMatch(projectFileName))
                    {
                        _itemMatches.Add(new Match(projectFile, pattern));
                        break;
                    }
                }
                              
            }
        }
    }
}
