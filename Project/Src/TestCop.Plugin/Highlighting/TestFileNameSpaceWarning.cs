// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2020
// --

using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace TestCop.Plugin.Highlighting
{
    [ConfigurableSeverityHighlighting(SeverityId, CSharpLanguage.Name)]
    [RegisterConfigurableSeverity(
        SeverityId,
        null, Highlighter.HighlightingGroup,
        "Namespace of file doesn't match its location",
        "Namespace of file doesn't match its location",
        Severity.WARNING)]
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
        private readonly VirtualFileSystemPath _targetFolder;
        
        public IAccessRightsOwnerDeclaration Declaration
        {
            get { return _declaration; }
        }

        public TestFileNameSpaceWarning(IProjectItem offendingProjectItem, IAccessRightsOwnerDeclaration declaration
            , string expectedNameSpace
            , IProject targetProject, VirtualFileSystemPath targetFolder)
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

        public VirtualFileSystemPath TargetFolder
        {
            get { return _targetFolder; }
        }
    }
}