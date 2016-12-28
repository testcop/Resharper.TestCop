// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;
using TestCop.Plugin.Highlighting;

[assembly: RegisterConfigurableSeverity(
        MethodShouldBePublicWarning.SeverityId,
        null, Highlighter.HighlightingGroup,
        "Test method should be public",
        "TestCop : Method with testing attributes should be public",
        Severity.ERROR)]

namespace TestCop.Plugin.Highlighting
{
    [ConfigurableSeverityHighlighting(SeverityId, CSharpLanguage.Name)]
    public class MethodShouldBePublicWarning : AbstractShouldBePublicWarning
    {
        internal const string SeverityId = "MethodShouldBePublic";

        public MethodShouldBePublicWarning(string attributeName, IAccessRightsOwnerDeclaration declaration)
            : base(SeverityId, string.Format("Methods with [{0}] must be public.", attributeName), declaration)
        {
        }       
    }
}