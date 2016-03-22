// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon.CSharp.Stages;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;

namespace TestCop.Plugin
{
    public class TestFileAnalysisDaemonStageProcess : CSharpDaemonStageProcessBase
    {
        private readonly IDaemonProcess _myDaemonProcess;
        private readonly IContextBoundSettingsStore _settings;

        public TestFileAnalysisDaemonStageProcess(IDaemonProcess daemonProcess, IContextBoundSettingsStore settings, ICSharpFile file)
            : base(daemonProcess, file)
        {
            _myDaemonProcess = daemonProcess;            
            _settings = settings;            
        }

        private static bool _mappedOnceThisSession;

        public override void Execute(Action<DaemonStageResult> commiter)
        {
            if (!_mappedOnceThisSession)
            {
                //not a nice solution but I needed a way to ensure the testcop key mappings are put in place
                _mappedOnceThisSession = true;
                ResharperHelper.ForceKeyboardBindings();
            }
            
            if (File.GetProject().IsTestProject() == false) 
                return;//only apply rules with projects we recognise as test projects

            // Running visitor against the PSI
            var elementProcessor = new TestFileAnalysisElementProcessor(this, _myDaemonProcess, _settings);
            File.ProcessDescendants(elementProcessor);

            // Checking if the daemon is interrupted by user activity
            if (_myDaemonProcess.InterruptFlag)
                throw new ProcessCancelledException();

            // Commit the result into document
            commiter(new DaemonStageResult(elementProcessor.Highlightings));
        }
    }
}
