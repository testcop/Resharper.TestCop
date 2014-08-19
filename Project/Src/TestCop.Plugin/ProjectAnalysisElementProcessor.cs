// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.I18n.Services;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin
{
    public class ProjectAnalysisElementProcessor : IRecursiveElementProcessor
    {
        private readonly IDaemonProcess _process;
        private readonly IContextBoundSettingsStore _settings;        
        private readonly List<HighlightingInfo> _myHighlightings = new List<HighlightingInfo>();

        public List<HighlightingInfo> Highlightings
        {
            get { return _myHighlightings; }
        }

        public ProjectAnalysisElementProcessor(IDaemonProcess process, IContextBoundSettingsStore settings)
        {
            _process = process;
            _settings = settings;            
        }
        
        public bool InteriorShouldBeProcessed(ITreeNode element)
        {
            return false;
        }

        public void ProcessBeforeInterior(ITreeNode element)
        {
        }

        public void ProcessAfterInterior(ITreeNode element)
        {
            if (String.Compare(_process.SourceFile.Name, "AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase)!=0) return;

            var usingBlock = element as IUsingList;
            if (usingBlock != null)
            {
                CheckForProjectFilesNotInProjectAndWarn(element);                
            }                    
        }
   
        public bool ProcessingIsFinished
        {
            get {  return _process.InterruptFlag; }
        }

        private void CheckForProjectFilesNotInProjectAndWarn(ITreeNode element)
        {
            var currentProject = element.GetProject();

            var allProjectFileLocations = currentProject.GetAllProjectFiles().Select(p => p.Location).ToList();
            var allProjectFiles = allProjectFileLocations.Select(loc => loc.FullPath).ToList();
            var allProjectFolders = allProjectFileLocations.Select(loc => loc.Directory.FullPath).Distinct();

            var filesOnDisk =
                allProjectFolders.SelectMany(
                    directory => new System.IO.DirectoryInfo(directory).EnumerateFiles("*.cs",
                        System.IO.SearchOption.TopDirectoryOnly).Select(f => f));

            foreach (var fileOnDisk in filesOnDisk)
            {
                if (allProjectFiles.Any(x => String.Compare(x, fileOnDisk.FullName,  StringComparison.OrdinalIgnoreCase) == 0)) continue;

                IHighlighting highlighting = new FileNotPartOfProjectWarning(currentProject, fileOnDisk);
                _myHighlightings.Add(new HighlightingInfo(element.GetDocumentRange(), highlighting));
            }
        }       
    }
}