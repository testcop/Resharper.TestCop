using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Psi.Tree;

namespace TestCop.Plugin.Highlighting
{
    [StaticSeverityHighlighting(Severity.WARNING, Highlighter.HighlightingGroup)]
    public class TestFileNameSpaceWarning : CSharpHighlightingBase, IHighlighting
    {        
        private readonly IAccessRightsOwnerDeclaration _declaration;
        private readonly string _expectedNameSpace;

        public IAccessRightsOwnerDeclaration Declaration
        {
            get { return _declaration; }
        }

        public TestFileNameSpaceWarning(IAccessRightsOwnerDeclaration declaration, string expectedNameSpace)
        {
            
            _declaration = declaration;
            _expectedNameSpace = expectedNameSpace;
        }

        public override bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get { return string.Format("Namespace of test expected to be {0}", _expectedNameSpace); }
        }

        public string ErrorStripeToolTip
        {
            get { return ToolTip; }
        }

        public int NavigationOffsetPatch
        {
            get { return 0; }
        }        
    }
}