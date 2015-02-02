using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp.Errors;

namespace TestCop.Plugin.Highlighting
{
    [StaticSeverityHighlighting(Severity.WARNING, Highlighter.HighlightingGroup)]
    class TestMethodMissingCodeWarning : CSharpHighlightingBase, IHighlighting
    {
        internal const string SeverityId = "TestMethodMissingCodeWarning";

        private readonly string _tooltipString;

        public TestMethodMissingCodeWarning(string tooltipString)
        {
            _tooltipString = tooltipString;
        }

        public override bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get { return _tooltipString; }
        }

        public string ErrorStripeToolTip
        {
            get
            {
                return ToolTip;
            }
        }

        public int NavigationOffsetPatch
        {
            get
            {
                return 0;
            }
        }
    }
}
