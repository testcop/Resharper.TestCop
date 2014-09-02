// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace TestCop.Plugin
{
    public class ProjectAnalysisDaemonStageProcess : IDaemonStageProcess
    {
        private readonly IDaemonProcess _myDaemonProcess;
        private readonly IContextBoundSettingsStore _settings;

        public ProjectAnalysisDaemonStageProcess(IDaemonProcess daemonProcess, IContextBoundSettingsStore settings)
        {
            _myDaemonProcess = daemonProcess;            
            _settings = settings;            
        }

        public void Execute(Action<DaemonStageResult> commiter)
        {
            // Getting PSI (AST) for the file being highlighted            
            var file = _myDaemonProcess.SourceFile.GetTheOnlyPsiFile(CSharpLanguage.Instance) as ICSharpFile;
            if (file == null)
                return;

            if (!Settings.FindOrphanedProjectFiles) return;

            //Only do the analysis on a 'key' file - as results can't be attached at a project level
            if (String.Compare(_myDaemonProcess.SourceFile.Name, "AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase) != 0) 
                return;

            // Running visitor against the PSI
            var elementProcessor = new ProjectAnalysisElementProcessor(_myDaemonProcess, _settings);
            file.ProcessDescendants(elementProcessor);

            // Checking if the daemon is interrupted by user activity
            if (_myDaemonProcess.InterruptFlag)
                throw new ProcessCancelledException();

            // Commit the result into document
            commiter(new DaemonStageResult(elementProcessor.Highlightings));
        }

        public IDaemonProcess DaemonProcess
        {
            get { return _myDaemonProcess; }
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
