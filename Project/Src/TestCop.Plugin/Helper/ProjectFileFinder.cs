// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2017
// --

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper
{    
    public class ProjectFileFinder: RecursiveProjectVisitor
    {
        private readonly List<IProjectFile> _items;
        private readonly Regex[] _regexs;

        public ProjectFileFinder(List<IProjectFile> items, params Regex[] regexs)
        {
            _items = items;
            _regexs = regexs;
        }

        public override void VisitProjectFile(IProjectFile projectFile)
        {
            base.VisitProjectFile(projectFile);
            string projectFileName = projectFile.Location.NameWithoutExtension;

            if (projectFile.Kind == ProjectItemKind.PHYSICAL_FILE)
            {
                if (_regexs.Any(regex => regex.IsMatch(projectFileName)))
                {
                    _items.AddIfMissing(projectFile,(f1, f2) => f1.Location.FullPath == f2.Location.FullPath );
                }               
            }
        }
    }
}
