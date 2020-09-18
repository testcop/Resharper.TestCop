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
        SeverityId,
        null, Highlighter.HighlightingGroup,
        "Test class name should match file name",
        "TestCop : The name of the test file should match the test class name it contains",
        Severity.ERROR)]
    public class TestClassNameDoesNotMatchFileNameWarning : AbstractTestClassNameWarning
    {
        internal const string SeverityId = "TestClassNameDoesNotMatchFileNameWarning";

        public TestClassNameDoesNotMatchFileNameWarning(string declaredClassName, string testClassNameFromFileName, IAccessRightsOwnerDeclaration declaration)
            : base(string.Format("Test classname and filename are not in sync {0}<>{1}.", declaredClassName, testClassNameFromFileName), declaration)
        {              
        }
    }
}