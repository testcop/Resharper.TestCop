using JetBrains.ReSharper.Feature.Services.CSharp.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using TestCop.Plugin.Highlighting;

[assembly: RegisterConfigurableSeverity(
    TestMethodMissingCodeWarning.SeverityId,
    null, Highlighter.HighlightingGroup,
    "Test methods should contain code",
    "TestCop : All tests methods should test something",
    Severity.WARNING)]

namespace TestCop.Plugin.Highlighting
{

    [ConfigurableSeverityHighlighting(SeverityId, CSharpLanguage.Name)]
    class TestMethodMissingCodeWarning : HighlightingBase, IHighlighting
    {
        internal const string SeverityId = "TestMethodMissingCodeWarning";

        private readonly ICSharpFunctionDeclaration _declaration;
        private readonly string _tooltipString;

        public TestMethodMissingCodeWarning(ICSharpFunctionDeclaration declaration, string tooltipString)
        {
            _declaration = declaration;
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
