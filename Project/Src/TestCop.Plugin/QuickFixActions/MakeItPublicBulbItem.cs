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
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Intentions.Extensibility.Menu;
using JetBrains.ReSharper.Psi;
using JetBrains.TextControl;
using JetBrains.Util;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.QuickFixActions
{

    //http://hadihariri.com/tag/resharper/page/3/
    //http://code.google.com/p/agentsmithplugin/source/browse/branches/R%237.0/src/AgentSmith/SpellCheck/ReplaceWordWithBulbItem.cs?spec=svn316&r=316
    [QuickFix]
    public class MakeItPublicBulbItem : BulbActionBase, IQuickFix
    {
        private readonly AbstractShouldBePublicWarning _highlight;

        public MakeItPublicBulbItem(AbstractShouldBePublicWarning highlight)
        {
            _highlight = highlight;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            _highlight.Declaration.SetAccessRights(AccessRights.PUBLIC);
            return null;
        }

        public override string Text
        {
            get { return String.Format("Make public"); }
        }
#if R7
        public void CreateBulbItems(BulbMenu menu, Severity severity)
        {
            menu.ArrangeQuickFix(this, Severity.ERROR); ;
        }
#else     
        public IEnumerable<IntentionAction> CreateBulbItems()
        {
            foreach (IntentionAction intentionAction in  BulbActionExtensions.ToQuickFixAction(this))
                yield return intentionAction;
        }
#endif
        public bool IsAvailable(IUserDataHolder cache)
        {
            return _highlight.IsValid();
        }
    }
}
