// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;

namespace TestCop.Plugin.Highlighting
{
    [ConfigurableSeverityHighlighting(SeverityId, CSharpLanguage.Name)]
    public class ClassShouldBePublicWarning : AbstractShouldBePublicWarning
    {
        internal const string SeverityId = "ClassShouldBePublic";

        public ClassShouldBePublicWarning(string attributeName, IAccessRightsOwnerDeclaration declaration)
            : base(string.Format("Types with [{0}] must be public.", attributeName), declaration)
        {
        }
    }
}