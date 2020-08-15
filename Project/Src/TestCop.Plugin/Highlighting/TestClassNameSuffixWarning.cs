// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2020
// --
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;

namespace TestCop.Plugin.Highlighting
{
    [ConfigurableSeverityHighlighting(SeverityId, CSharpLanguage.Name)]
    [RegisterConfigurableSeverity(
        TestClassNameSuffixWarning.SeverityId,
        null, Highlighter.HighlightingGroup,
        "All test classes should have the same suffix",
        "TestCop : To easily identify a test class by its name it must have the configured suffix",
        Severity.ERROR)]
    public class TestClassNameSuffixWarning : AbstractTestClassNameWarning
    {
        internal const string SeverityId = "TestClassNameSuffixWarning";

        public TestClassNameSuffixWarning(string expectedSuffix, IAccessRightsOwnerDeclaration declaration)
            : base(string.Format("Test class names should end with '{0}'.", expectedSuffix), declaration)
        {
        }      
    }
}