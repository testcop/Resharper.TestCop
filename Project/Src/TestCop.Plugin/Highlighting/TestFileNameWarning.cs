// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;
using TestCop.Plugin.Highlighting;

[assembly: RegisterConfigurableSeverity(
    TestFileNameWarning.SeverityId,
    null, Highlighter.HighlightingGroup,
    "The file name for the test does not match testcop rules would be ClassATests.cs or ClassA.SecurityTests.cs or ClassA.SecurityIntegrationTests.cs",
    "Consistent naming aids code navigation and refactorings",
    Severity.WARNING)]

namespace TestCop.Plugin.Highlighting
{
    [ConfigurableSeverityHighlighting(SeverityId, CSharpLanguage.Name)]
    public class TestFileNameWarning : HighlightingBase, IHighlighting
    {
        internal const string SeverityId = "TestFileNameWarning";
        private readonly string _tooltipString;
        private readonly IAccessRightsOwnerDeclaration _declaration;

        public IAccessRightsOwnerDeclaration Declaration
        {
            get { return _declaration; }
        }

        public TestFileNameWarning(string toolTip, IAccessRightsOwnerDeclaration declaration)
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