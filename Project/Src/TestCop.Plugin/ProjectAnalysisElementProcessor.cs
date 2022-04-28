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
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.PsiGen.Util;
using JetBrains.Util;

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
            if (element is IUsingList)
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
            ICollection<VirtualFileSystemPath> directoriesToSkip = currentProject.GetOutputDirectories();
            directoriesToSkip.AddAll(currentProject.GetIntermediateDirectories());

            var allProjectFileLocations = currentProject.GetAllProjectFiles().Select(p => p.Location).ToList();
            var allProjectFiles = allProjectFileLocations.Select(loc => loc.FullPath).ToList();
            var allProjectFolders = allProjectFileLocations.Select(loc => loc.Directory).Distinct();

            allProjectFolders = allProjectFolders.Where(x => !InDirectory(directoriesToSkip, x));

            var filesOnDisk = new List<FileInfo>();
            foreach (string regex in filesToFind)
            {
                filesOnDisk.AddRange(
                    allProjectFolders.SelectMany(
                        directory =>
                            new System.IO.DirectoryInfo(directory.FullPath)
                                .EnumerateFiles(regex, System.IO.SearchOption.TopDirectoryOnly)
                                .Select(f => f))
                );
            }

            var orphanedFiles = new List<FileInfo>();

            foreach (var fileOnDisk in filesOnDisk)
            {
                if (allProjectFiles.Any(
                    x => String.Compare(x, fileOnDisk.FullName, StringComparison.OrdinalIgnoreCase) == 0)) continue;
                
                orphanedFiles.Add(fileOnDisk);
            }

            if (orphanedFiles.Count > 0)
            {
                IHighlighting highlighting = new FilesNotPartOfProjectWarning(currentProject, orphanedFiles);
                AddHighlighting(element.GetDocumentRange(), highlighting);
            }
        }

        public static bool InDirectory(ICollection<VirtualFileSystemPath> directoriesToSkip, VirtualFileSystemPath fileOnDisk)
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