using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Psi.Tree;

namespace TestCop.Highlighting
{
    [StaticSeverityHighlighting(Severity.ERROR, Highlighter.HighlightingGroup)]
    public class ShouldBePublicWarning :CSharpHighlightingBase,IHighlighting
    {
        private readonly string _tooltipString;
        private readonly IAccessRightsOwnerDeclaration _declaration;

        public IAccessRightsOwnerDeclaration Declaration
        {
            get { return _declaration; }
        }

        public ShouldBePublicWarning(string toolTip, IAccessRightsOwnerDeclaration declaration)
        {
            _tooltipString = toolTip;
            _declaration = declaration;
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
            get { return _tooltipString; }
        }

        public int NavigationOffsetPatch
        {
            get { return 0; }
        }
    }
}