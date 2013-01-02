using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;

namespace TestCop.Plugin.Helper
{
    //http://devnet.jetbrains.net/message/5171924#5171924

    public class ProjectFileFinder2: RecursiveProjectVisitor
    {
        private readonly List<IProjectFile> _items;
        private readonly Regex _regex;

        public ProjectFileFinder2(List<IProjectFile> items, Regex regex)
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

    public class ProjectFileFinder 
    {
        private readonly List<IProjectItem> _items;
        private readonly Regex _regex;

        public ProjectFileFinder(List<IProjectItem> items, Regex regex)
        {
            _items = items;
            _regex = regex;
        }

        
   
        private void Check(IProjectItem projectItem)
        {            
            if (projectItem.Kind == ProjectItemKind.PHYSICAL_FILE)
            {
                var name = projectItem.Location.NameWithoutExtension;
                if (_regex.IsMatch(name))
                {
                    _items.Add(projectItem);
                }
            }
        }

        public void Visit(IProjectFile projectFile)
        {            
            var project = projectFile.GetProject();
            if (project != null)
            {
               foreach (var pi in project.GetAllProjectFiles())
               {
                   Check(pi);
               }
            }
        }
    }
}
