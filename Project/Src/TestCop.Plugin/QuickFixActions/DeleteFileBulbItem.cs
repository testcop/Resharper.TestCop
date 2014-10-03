// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --
 
using System;
using System.Collections.Generic;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Extensibility.Menu;
using JetBrains.TextControl;
using JetBrains.Util;
using TestCop.Plugin.Helper;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.QuickFixActions
{
    [QuickFix]
    public class DeleteFileBulbItem : BulbActionBase, IQuickFix
    {
        private readonly FileNotPartOfProjectWarning _highlight;

        public DeleteFileBulbItem(FileNotPartOfProjectWarning highlight)
        {
            _highlight = highlight;
        }
        
        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        { 
            using (var notusedCookie = solution.CreateTransactionCookie(DefaultAction.Rollback, this.GetType().Name, progress))
            {
                _highlight.FileOnDisk.Delete();
                DTEHelper.RefreshSolutionExplorerWindow();
            }
            return null;
        }

        public override string Text
        {
            get
            {
                return String.Format("Delete file [{0}]", _highlight.OffendingFileDisplayableName);
            }
        }

        public IEnumerable<IntentionAction> CreateBulbItems()
        {
            foreach (IntentionAction intentionAction in  this.ToQuickFixAction())
                yield return intentionAction;
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            return _highlight.IsValid();
        }
    }
}
