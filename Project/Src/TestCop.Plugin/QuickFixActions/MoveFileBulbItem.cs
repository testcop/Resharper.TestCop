// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using System.Collections.Generic;
using JetBrains.Application.DataContext;
using JetBrains.Application.Progress;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers.impl;
using JetBrains.DocumentManagers.Transactions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Intentions;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Feature.Services.Refactorings;
using JetBrains.ReSharper.Refactorings.Move.MoveToFolder;
using JetBrains.ReSharper.Refactorings.Move.MoveToFolder.Impl;
using JetBrains.TextControl;
using JetBrains.Util;
using TestCop.Plugin.Highlighting;
using JetBrains.ReSharper.Resources.Shell;

namespace TestCop.Plugin.QuickFixActions
{    
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
            var projectFile = (IProjectFile) _highlight.OffendingProjectItem;
       
            using (var cookie = solution.CreateTransactionCookie(DefaultAction.Rollback, this.GetType().Name, progress))
            {
                IProjectFolder newFolder = (IProjectFolder)_highlight.TargetProject.FindProjectItemByLocation(_highlight.TargetFolder)
                    ?? _highlight.TargetProject.GetOrCreateProjectFolder(_highlight.TargetFolder, cookie); 

                var workflow = new MoveToFolderWorkflow(solution, "ManualMoveToFolderQuickFix");
                IProjectFolder targetFolder = newFolder ?? _highlight.TargetProject;

                var dataProvider = new MoveToFolderDataProvider(true, false, targetFolder, new List<string>(), new List<string>());
                workflow.SetDataProvider(dataProvider);
              
                Lifetimes.Using(
                    (lifetime => WorkflowExecuter.ExecuteWithCustomHost(
                        JetBrains.ActionManagement.ShellComponentsEx.ActionManager(Shell.Instance.Components)
                        .DataContexts.CreateWithoutDataRules(lifetime
                        , DataRules.AddRule(DataRules.AddRule("ManualMoveToFolderQuickFix"
                        , JetBrains.ProjectModel.DataContext.ProjectModelDataConstants.PROJECT_MODEL_ELEMENTS, new IProjectModelElement[]{projectFile})
                        , "ManualMoveToFolderQuickFix"
                        , JetBrains.ProjectModel.DataContext.ProjectModelDataConstants.SOLUTION, solution))
                        , workflow, new SimpleWorkflowHost())));
                                            
                cookie.Commit(progress);
            }

            return null;
        }

        public override string Text
        {
            get { return String.Format("Move file file to : " + _highlight.ExpectedNameSpace); }
        }

        public IEnumerable<IntentionAction> CreateBulbItems()
        {
            foreach (IntentionAction intentionAction in BulbActionExtensions.ToQuickFixIntentions(this, null, UnnamedThemedIcons.Agent16x16.Id))
              yield return intentionAction;
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            return _highlight.IsValid();
        }      
    }
    
}
