// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --
 
using System;
using System.Collections.Generic;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.I18n.Services;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Extensibility.Menu;
using JetBrains.TextControl;
using JetBrains.Util;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.QuickFixActions
{
    [QuickFix]
    public class AddExistingFileToProjectBulbItem : BulbActionBase, IQuickFix
    {
        private readonly FileNotPartOfProjectWarning _highlight;

        public AddExistingFileToProjectBulbItem(FileNotPartOfProjectWarning highlight)
        {
            _highlight = highlight;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (var cookie = solution.CreateTransactionCookie(DefaultAction.Rollback, this.GetType().Name, progress))
            {
                _highlight.CurrentProject.AddExistenFile(FileSystemPath.Parse(_highlight.FileOnDisk.FullName), cookie);
                cookie.Commit(progress);
            }

            return null;
        }

        public override string Text
        {
            get
            {
                return String.Format("Add file to project [{0}]", _highlight.OffendingFileDisplayableName);
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
