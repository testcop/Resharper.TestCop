using System.IO;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Highlighting
{
    [StaticSeverityHighlighting(Severity.WARNING, Highlighter.HighlightingGroup)]
    public class FileNotPartOfProjectWarning : CSharpHighlightingBase, IHighlighting
    {
        private readonly IProject _currentProject;
        private readonly FileInfo _fileOnDisk;
        internal const string SeverityId = "FileNotPartOfProjectWarning";

    
        public FileNotPartOfProjectWarning(IProject currentProject, FileInfo fileOnDisk)
        {
            _currentProject = currentProject;
            _fileOnDisk = fileOnDisk;
        }

        public string OffendingFileDisplayableName
        {
            get
            {
                 return FileOnDisk.FullName.RemoveLeading(CurrentProject.Location.FullPath);
            }
        }

        public override bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get { return string.Format("File not part of project: [{0}]", OffendingFileDisplayableName);  }
        }

        public string ErrorStripeToolTip
        {
            get
            {
                return ToolTip;
            }
        }

        public int NavigationOffsetPatch
        {
            get
            {
                return 0;
            }
        }

        public IProject CurrentProject
        {
            get { return _currentProject; }
        }

        public FileInfo FileOnDisk
        {
            get { return _fileOnDisk; }
        }
    }
}
