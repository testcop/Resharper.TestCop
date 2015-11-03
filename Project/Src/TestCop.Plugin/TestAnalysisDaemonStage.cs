// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2015
// --

using System;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Feature.Services.CSharp.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace TestCop.Plugin
{
    [DaemonStage]
    public class TestAnalysisDaemonStage : CSharpDaemonStageBase
    {
        /// <summary>
        /// Daemon stage for analysis. This class is automatically loaded by ReSharper daemon 
        /// because it's marked with the attribute.
        /// </summary>
        protected override IDaemonStageProcess CreateProcess(IDaemonProcess process, IContextBoundSettingsStore settings,
            DaemonProcessKind processKind, ICSharpFile file)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            return new TestFileAnalysisDaemonStageProcess(process,settings, file);
        }             
    }

    [DaemonStage]
    public class TestAnalysisProjectDaemonStage : CSharpDaemonStageBase
    {
        /// <summary>
        /// Daemon stage for analysis. This class is automatically loaded by ReSharper daemon 
        /// because it's marked with the attribute.
        /// </summary>
        protected override IDaemonStageProcess CreateProcess(IDaemonProcess process, IContextBoundSettingsStore settings,
            DaemonProcessKind processKind, ICSharpFile file)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            return new ProjectAnalysisDaemonStageProcess(process, settings, file);        
        }
    }
}
