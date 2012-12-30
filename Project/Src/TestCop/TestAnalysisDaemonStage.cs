using System;
using System.Collections.Generic;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi;

namespace TestCop.Plugin
{
    /// <summary>
    /// Daemon stage for analysis. This class is automatically loaded by ReSharper daemon 
    /// because it's marked with the attribute.
    /// </summary>
    [DaemonStage]
    public class TestAnalysisDaemonStage : IDaemonStage
    {        
        /// <summary>
        /// This method provides a <see cref="IDaemonStageProcess"/> instance which is assigned to highlighting a single document
        /// </summary>
        public IEnumerable<IDaemonStageProcess> CreateProcess(IDaemonProcess process, IContextBoundSettingsStore settings, DaemonProcessKind kind)
        {
            if (process == null)
                throw new ArgumentNullException("process");
                       
            return new[]{new TestFileAnalysisDaemonStageProcess(process,settings)};
        }

        public ErrorStripeRequest NeedsErrorStripe(IPsiSourceFile sourceFile, IContextBoundSettingsStore settings)
        {
            // We want to add markers to the right-side stripe as well as contribute to document errors
            return ErrorStripeRequest.STRIPE_AND_ERRORS;
        }
    }
}
