// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using TestCop.Plugin.Helper;

namespace TestCop.Plugin
{
    public class TestFileAnalysisDaemonStageProcess : IDaemonStageProcess
    {
        private readonly IDaemonProcess _myDaemonProcess;
        private readonly IContextBoundSettingsStore _settings;

        public TestFileAnalysisDaemonStageProcess(IDaemonProcess daemonProcess, IContextBoundSettingsStore settings)
        {
            _myDaemonProcess = daemonProcess;            
            _settings = settings;            
        }

        private static bool _mappedOnceThisSession;

        public void Execute(Action<DaemonStageResult> commiter)
        {
            if (!_mappedOnceThisSession)
            {
                //not a nice solution but I needed a way to ensure the testcop key mappings are put in place
                _mappedOnceThisSession = true;
                ResharperHelper.ForceKeyboardBindings();
            }
            // Getting PSI (AST) for the file being highlighted            
            var file = _myDaemonProcess.SourceFile.GetTheOnlyPsiFile(CSharpLanguage.Instance) as ICSharpFile;
            if (file == null)
                return;

            // Running visitor against the PSI
            var elementProcessor = new TestFileAnalysisElementProcessor(_myDaemonProcess, _settings);
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
    }
}
