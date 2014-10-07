using System.Collections.Generic;
using System.IO;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Highlighting
{
    [StaticSeverityHighlighting(Severity.WARNING, Highlighter.HighlightingGroup)]
    public class FilesNotPartOfProjectWarning : CSharpHighlightingBase, IHighlighting
    {
        private readonly IProject _currentProject;
        private readonly IList<FileInfo> _fileOnDisk;
        internal const string SeverityId = "FilesNotPartOfProjectWarning";

    
        public FilesNotPartOfProjectWarning(IProject currentProject, IList<FileInfo> fileOnDisk)
        {
            _currentProject = currentProject;
            _fileOnDisk = fileOnDisk;
        }
        
        public override bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get
            {
                if(_fileOnDisk.Count==1)
                    return string.Format("File not part of project: [{0}]", _fileOnDisk[0].FullName.RemoveLeading(_currentProject.Location.FullPath));
                return string.Format("Files not part of project [{0} Files]", _fileOnDisk.Count);                   
            }
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

        public IList<FileInfo> FileOnDisk
        {
            get { return _fileOnDisk; }
        }
    }
}
