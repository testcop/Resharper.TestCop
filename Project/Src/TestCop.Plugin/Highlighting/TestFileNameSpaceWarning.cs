// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using TestCop.Plugin.Highlighting;

[assembly: RegisterConfigurableSeverity(
    TestFileNameSpaceWarning.SeverityId,
    null, Highlighter.HighlightingGroup,
    "Namespace of file doesn't match its location",
    "Namespace of file doesn't match its location",
    Severity.WARNING)]

namespace TestCop.Plugin.Highlighting
{
    
    [ConfigurableSeverityHighlighting(SeverityId, CSharpLanguage.Name)]
    public class TestFileNameSpaceWarning : HighlightingBase, IHighlighting
    {
        internal const string SeverityId = "TestFileNameSpaceWarning";

        private readonly IProjectItem _offendingProjectItem;

        public IProjectItem OffendingProjectItem
        {
            get { return _offendingProjectItem; }
        }

        private readonly IAccessRightsOwnerDeclaration _declaration;
        private readonly string _expectedNameSpace;

        public string ExpectedNameSpace
        {
            get { return _expectedNameSpace; }
        }

        private readonly IProject _targetProject;
        private readonly FileSystemPath _targetFolder;
        
        public IAccessRightsOwnerDeclaration Declaration
        {
            get { return _declaration; }
        }

        public TestFileNameSpaceWarning(IProjectItem offendingProjectItem, IAccessRightsOwnerDeclaration declaration
            , string expectedNameSpace
            , IProject targetProject, FileSystemPath targetFolder)
        {
            _offendingProjectItem = offendingProjectItem;
            _declaration = declaration;
            _expectedNameSpace = expectedNameSpace;
            _targetProject = targetProject;
            _targetFolder = targetFolder;
        }

        public override bool IsValid()
        {
            return true;
        }

        public string ToolTip
        {
            get { return string.Format("Namespace of test expected to be {0}", _expectedNameSpace); }
        }

        public string ErrorStripeToolTip
        {
            get { return ToolTip; }
        }

        public int NavigationOffsetPatch
        {
            get { return 0; }
        }

        public IProject TargetProject
        {
            get { return _targetProject; }
        }

        public FileSystemPath TargetFolder
        {
            get { return _targetFolder; }
        }
    }
}