using System;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace TestCop
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

        public void Execute(Action<DaemonStageResult> commiter)
        {
            // Getting PSI (AST) for the file being highlighted
            var file = _myDaemonProcess.SourceFile.GetNonInjectedPsiFile<CSharpLanguage>() as ICSharpFile;
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
