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
        ClassShouldBePublicWarning.SeverityId,
        null, Highlighter.HighlightingGroup,
        "Test class should be public",
        "TestCop : Class with testing attributes should be public",
        Severity.ERROR)]

namespace TestCop.Plugin.Highlighting
{
    [ConfigurableSeverityHighlighting(SeverityId, CSharpLanguage.Name)]
    public class ClassShouldBePublicWarning : AbstractShouldBePublicWarning
    {
        internal const string SeverityId = "ClassShouldBePublic";

        public ClassShouldBePublicWarning(string attributeName, IAccessRightsOwnerDeclaration declaration)
            : base(SeverityId, string.Format("Types with [{0}] must be public.", attributeName), declaration)
        {
        }      
    }
}