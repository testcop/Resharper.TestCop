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
    public class TestClassNameSuffixWarning : AbstractTestClassNameWarning
    {
        internal const string SeverityId = "TestClassNameSuffixWarning";

        public TestClassNameSuffixWarning(string expectedSuffix, IAccessRightsOwnerDeclaration declaration)
            : base(string.Format("Test class names should end with '{0}'.", expectedSuffix), declaration)
        {
        }
    }
}