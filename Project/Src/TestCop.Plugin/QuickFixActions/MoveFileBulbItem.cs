// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2014
// --

namespace TestCop.Plugin.QuickFixActions
{
    using System;
    using System.Collections.Generic;

    using JetBrains.Application.DataContext;
    using JetBrains.Application.Progress;
    using JetBrains.Application.UI.Actions.ActionManager;
    using JetBrains.DocumentManagers.impl;
    using JetBrains.DocumentManagers.Transactions;
    using JetBrains.Lifetimes;
    using JetBrains.ProjectModel;
    using JetBrains.ProjectModel.DataContext;
    using JetBrains.ReSharper.Feature.Services.Bulbs;
    using JetBrains.ReSharper.Feature.Services.Intentions;
    using JetBrains.ReSharper.Feature.Services.QuickFixes;
    using JetBrains.ReSharper.Feature.Services.Refactorings;
    using JetBrains.ReSharper.Refactorings.Move.MoveToFolder;
    using JetBrains.ReSharper.Refactorings.Move.MoveToFolder.Impl;
    using JetBrains.ReSharper.Resources.Shell;
    using JetBrains.TextControl;
    using JetBrains.Util;

    using TestCop.Plugin.Highlighting;

    [QuickFix]
    public class MoveFileBulbItem : BulbActionBase, IQuickFix
    {
        private const string ManualMoveToFolderActionId = "ManualMoveToFolderQuickFix";
        private readonly TestFileNameSpaceWarning _highlight;

        public MoveFileBulbItem(TestFileNameSpaceWarning highlight)
        {
            this._highlight = highlight;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            IProjectFile projectFile = (IProjectFile)this._highlight.OffendingProjectItem;

            using (IProjectModelTransactionCookie cookie =
                solution.CreateTransactionCookie(DefaultAction.Rollback, this.GetType().Name, progress))
            {
                IProjectFolder newFolder =
                    (IProjectFolder)this._highlight.TargetProject.FindProjectItemByLocation(this._highlight.TargetFolder)
                    ?? this._highlight.TargetProject.GetOrCreateProjectFolder(this._highlight.TargetFolder, cookie);

                MoveToFolderWorkflow workflow = new MoveToFolderWorkflow(solution, ManualMoveToFolderActionId);
                IProjectFolder targetFolder = newFolder ?? this._highlight.TargetProject;

                MoveToFolderDataProvider dataProvider =
                    new MoveToFolderDataProvider(true, false, targetFolder, new List<string>(), new List<string>());
                workflow.SetDataProvider(dataProvider);

                Lifetime.Using(
                    lifetime => WorkflowExecuter.ExecuteWithCustomHost(
                        Shell.Instance.GetComponent<IActionManager>()
                            .DataContexts.CreateWithoutDataRules(lifetime
                                , DataRules.AddRule(ManualMoveToFolderActionId
                                    , ProjectModelDataConstants.PROJECT_MODEL_ELEMENTS,
                                    new IProjectModelElement[] { projectFile }).AddRule(ManualMoveToFolderActionId
                                    , ProjectModelDataConstants.SOLUTION, solution))
                        , workflow, new SimpleWorkflowHost()));

                cookie.Commit(progress);
            }

            return null;
        }

        public override string Text => $"Move file file to : {this._highlight.ExpectedNameSpace}";

        public IEnumerable<IntentionAction> CreateBulbItems()
        {
            foreach (IntentionAction intentionAction in this.ToQuickFixIntentions(null, UnnamedThemedIcons.Agent16x16.Id))
            {
                yield return intentionAction;
            }
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            return this._highlight.IsValid();
        }
    }
}