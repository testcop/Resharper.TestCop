// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2015
// --

using System;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon.CSharp.Stages;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.UnitTestFramework.DotNetCore.Exceptions;
using JetBrains.Util;

namespace TestCop.Plugin
{
    public class ProjectAnalysisDaemonStageProcess :  CSharpDaemonStageProcessBase
    {
        private readonly IDaemonProcess _myDaemonProcess;
        private readonly IContextBoundSettingsStore _settings;

        public ProjectAnalysisDaemonStageProcess(IDaemonProcess daemonProcess, IContextBoundSettingsStore settings, ICSharpFile csharpFile)
            : base(daemonProcess, csharpFile )
        {
            _myDaemonProcess = daemonProcess;            
            _settings = settings;            
        }

        public override void Execute(Action<DaemonStageResult> commiter)
        {           
            if (!Settings.FindOrphanedProjectFiles) return;

            //Only do the analysis on a 'key' file - as results can't be attached at a project level
            if (String.Compare(_myDaemonProcess.SourceFile.Name, "AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase) != 0) 
                return;

            // Running visitor against the PSI            
            var elementProcessor = new ProjectAnalysisElementProcessor(this, _myDaemonProcess, _settings);
            File.ProcessDescendants(elementProcessor);

            // Checking if the daemon is interrupted by user activity
            if (_myDaemonProcess.InterruptFlag)
                throw new ProcessExitedUnexpectedlyException();

            // Commit the result into document
            commiter(new DaemonStageResult(elementProcessor.Highlightings));            
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
