using System.Collections.Generic;
using System.IO;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Highlighting;

[assembly: RegisterConfigurableSeverity(
        FilesNotPartOfProjectWarning.SeverityId,
        null, Highlighter.HighlightingGroup,
        "Orphaned file not part of project",
        "TestCop : All code files should be part of project",
        Severity.WARNING)]

namespace TestCop.Plugin.Highlighting
{    
    [ConfigurableSeverityHighlighting(SeverityId, CSharpLanguage.Name)]
    public class FilesNotPartOfProjectWarning : HighlightingBase, IHighlighting
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
