// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Daemon.CSharp.Errors;
using JetBrains.ReSharper.Feature.Services.CSharp.Daemon;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Tree;

namespace TestCop.Plugin.Highlighting
{
    public abstract class AbstractShouldBePublicWarning :HighlightingBase, IHighlighting
    {        
        private readonly string _severityId;
        private readonly string _tooltipString;
        private readonly IAccessRightsOwnerDeclaration _declaration;

        public IAccessRightsOwnerDeclaration Declaration
        {
            get { return _declaration; }
        }

        protected AbstractShouldBePublicWarning(string severityId, string toolTip, IAccessRightsOwnerDeclaration declaration)            
        {
            _severityId = severityId;
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