using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;

namespace TestCop.Plugin.Helper
{    
    public class ProjectFileFinder: RecursiveProjectVisitor
    {
        private readonly List<IProjectFile> _items;
        private readonly Regex _regex;

        public ProjectFileFinder(List<IProjectFile> items, Regex regex)
        {
            _items = items;
            _regex = regex;
        }

        public override void VisitProjectFile(IProjectFile projectFile)
        {
            base.VisitProjectFile(projectFile);
            string projectFileName = projectFile.Location.NameWithoutExtension;

            if (projectFile.Kind == ProjectItemKind.PHYSICAL_FILE)
            {
                if (_regex.IsMatch(projectFileName))
                {
                    _items.Add(projectFile);
                }
            }
        }
    }
}
