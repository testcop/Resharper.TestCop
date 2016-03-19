// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.Tree;

namespace TestCop.Plugin.Highlighting
{
    public abstract class AbstractTestClassNameWarning : IHighlighting
    {        
        private readonly string _tooltipString;
        private readonly IAccessRightsOwnerDeclaration _declaration;

        public IAccessRightsOwnerDeclaration Declaration
        {
            get { return _declaration; }
        }

        protected AbstractTestClassNameWarning(string toolTip, IAccessRightsOwnerDeclaration declaration)
        {            
            _tooltipString = toolTip;
            _declaration = declaration;
        }

        public bool IsValid()
        {
            if (Declaration != null)
                return Declaration.IsValid();
            return true;
        }

        public DocumentRange CalculateRange()
        {
            if (Declaration == null)
                return Declaration.GetNameDocumentRange();
            return Declaration.GetHighlightingRange();
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
