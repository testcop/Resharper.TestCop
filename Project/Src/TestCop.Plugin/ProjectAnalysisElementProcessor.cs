// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2019
// --

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon.CSharp.Stages;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.PsiGen.Util;
using JetBrains.Util;
using TestCop.Plugin.Helper;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin
{
    public class ProjectAnalysisElementProcessor : IRecursiveElementProcessor
    {
        private readonly IDaemonProcess _process;
        private readonly IContextBoundSettingsStore _settings;

        private readonly FilteringHighlightingConsumer _highlightingConsumer;

        protected void AddHighlighting(DocumentRange range, IHighlighting highlighting)
        {
            _highlightingConsumer.AddHighlighting(highlighting, range);
        }

        public IReadOnlyList<HighlightingInfo> Highlightings
        {
            get { return _highlightingConsumer.Highlightings.AsIReadOnlyList(); }
        }

        public ProjectAnalysisElementProcessor(ProjectAnalysisDaemonStageProcess stageProcess, IDaemonProcess process,
            IContextBoundSettingsStore settings)
        {
            _highlightingConsumer = new FilteringHighlightingConsumer(stageProcess.File.GetSourceFile(), stageProcess.File, settings);
            
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
            var usingBlock = element as IUsingList;
            if (usingBlock != null)
            {
                CheckForProjectFilesNotInProjectAndWarn(element);
            }
        }

        public bool ProcessingIsFinished
        {
            get { return _process.InterruptFlag; }
        }

        private void CheckForProjectFilesNotInProjectAndWarn(ITreeNode element)
        {
            string[] filesToFind = Settings.OrphanedFilesPatterns.Split('|');
            if (filesToFind.Length == 0) filesToFind = new[] {"*.cs"};

            var currentProject = element.GetProject();
            var directoriesToSkip = currentProject.GetOutputDirectories();
            directoriesToSkip.AddAll(currentProject.GetIntermidiateDirectories());

            var allProjectFileLocations = currentProject.GetAllProjectFiles().Select(p => p.Location).ToList();
            var allProjectFiles = allProjectFileLocations.Select(loc => loc.FullPath).ToList();
            var allProjectFolders = allProjectFileLocations.Select(loc => loc.Directory).Distinct();

            var filesOnDisk = new List<FileInfo>();
            filesToFind.ForEach(regex => filesOnDisk.AddRange(
                                    allProjectFolders.SelectMany(
                                        directory =>
                                            new System.IO.DirectoryInfo(directory.FullPath)
                                                .EnumerateFiles(regex, System.IO.SearchOption.TopDirectoryOnly)
                                                .Select(f => f))
                                ));


            var orphanedFiles = new List<FileInfo>();

            foreach (var fileOnDisk in filesOnDisk)
            {
                if (allProjectFiles.Any(
                    x => String.Compare(x, fileOnDisk.FullName, StringComparison.OrdinalIgnoreCase) == 0)) continue;
                
                if(!InDirectory(directoriesToSkip, FileSystemPath.TryParse(fileOnDisk.FullName)))
                    orphanedFiles.Add(fileOnDisk);
            }

            if (orphanedFiles.Count > 0)
            {
                IHighlighting highlighting = new FilesNotPartOfProjectWarning(currentProject, orphanedFiles);
                AddHighlighting(element.GetDocumentRange(), highlighting);
            }
        }

        public static bool InDirectory(ICollection<FileSystemPath> directoriesToSkip, FileSystemPath fileOnDisk)
        {
            if (fileOnDisk == null || directoriesToSkip == null) return false;
            return directoriesToSkip.Any(x => x.IsPrefixOf(fileOnDisk));
        }

    private TestFileAnalysisSettings Settings
        {
            get
            {
                var testFileAnalysisSettings = _settings.GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);
                return testFileAnalysisSettings;
            }
        }
    }
}