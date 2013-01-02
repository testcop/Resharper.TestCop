using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Daemon;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Extensibility.Menu;
using JetBrains.ReSharper.Psi;
using JetBrains.TextControl;
using JetBrains.Util;

namespace TestCop.QuickFixActions
{
    /*
    [QuickFix]
    public class MoveFileBulbItem : BulbActionBase, IQuickFix
    {
        private readonly TestFileNameSpaceWarning _highlight;

        public MoveFileBulbItem(TestFileNameSpaceWarning highlight)
        {
            _highlight = highlight;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            //Need to read more on MoveToFolderQuickFix()
            Template classTemplate = FileTemplatesManager.Instance.GetFileTemplatesForActions(context).Where(c => c.Shortcut == "Class").SingleOrDefault();
            IProjectFolder folder = (IProjectFolder) associatedProject.FindProjectItemByLocation(fileSystemPath)
                                    ?? AddNewItemUtil.GetOrCreateProjectFolder(associatedProject, fileSystemPath);

             
            return null;
        }

        public override string Text
        {
            get { return String.Format("#Move It#"); }
        }

        public void CreateBulbItems(BulbMenu menu, Severity severity)
        {
            menu.ArrangeQuickFix(this,Severity.ERROR);;
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            return _highlight.IsValid();
        }
    }
    */
}
