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
    public class TestClassNameDoesNotMatchFileNameWarning : AbstractTestClassNameWarning
    {
        internal const string SeverityId = "TestClassNameDoesNotMatchFileNameWarning";

        public TestClassNameDoesNotMatchFileNameWarning(string declaredClassName, string testClassNameFromFileName, IAccessRightsOwnerDeclaration declaration)
            : base(string.Format("Test classname and filename are not in sync {0}<>{1}.", declaredClassName, testClassNameFromFileName), declaration)
        {            
        }
    }
}