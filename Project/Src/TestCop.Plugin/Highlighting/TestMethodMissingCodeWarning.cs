// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2020
// --
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;

namespace TestCop.Plugin.Highlighting
{

    [ConfigurableSeverityHighlighting(SeverityId, CSharpLanguage.Name)]
    [RegisterConfigurableSeverity(
        TestMethodMissingCodeWarning.SeverityId,
        null, Highlighter.HighlightingGroup,
        "Test methods should contain code",
        "TestCop : All tests methods should test something",
        Severity.WARNING)]
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
