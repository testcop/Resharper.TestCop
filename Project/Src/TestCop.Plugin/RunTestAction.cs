// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2015
// --
using System.Collections.Generic;
using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestExplorer.Actions;
using JetBrains.ReSharper.UnitTestExplorer.Session;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Criteria;
using JetBrains.UI.ActionsRevised;
using JetBrains.UI.PopupMenu;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;

namespace TestCop.Plugin
{
    /*
    [Action("Testcop Run Unit Tests", Id = 92407+1, ShortcutScope = ShortcutScope.TextEditor, Icon = typeof(UnnamedThemedIcons.Agent16x16))]
    public class TestCopUnitTestRunContextAction : TestCopUnitTestRunContextActionBase
    {
        protected override IHostProviderDescriptor GetHostProviderDescriptor()
        {
            return UnitTestHost.Instance.GetProviderDescriptor("Process");
        }

        public override void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            IUnitTestSession session = this.GetSession(context);
            if (session == null || !session.IsIdle.Value)
                return;
            IHostProvider provider = this.GetHostProviderDescriptor().Provider;

            UnitTestElements elements = this.GetElementsToRun(session, context);

            if (elements != null && elements.Explicit.Count != 0)
            {
                //default behaviour if tests are present
                base.Execute(context, nextExecute);
                return;
            }

            JetPopupMenu menuAction=null;

            //Not the most elegant approach but avoids code duplication by using the main TestCop action
            var switchAction = (IExecutableAction) JumpToTestFileAction.CreateWith((menu, when) => menuAction=menu);
            switchAction.Execute(context, null);

            if (menuAction == null) return;
            
            var itms = menuAction.ItemKeys.Where(i => i is SimpleMenuItemForProjectItem) //only links to project files
                .Cast<SimpleMenuItemForProjectItem>() 
                .Select(i => i.AssociatedProjectItem).Where(i=>i.GetProject().IsTestProject()) //only files within test projects
                .ToList();

            if (itms.Count == 0)
            {
                ResharperHelper.AppendLineToOutputWindow("No associated test classes to run.");
                return;
            }

            var mgr = solution.GetComponent<UnitTestManager>();
            var unitTestElements = new List<IUnitTestElement>();

            unitTestElements.AddRange(mgr.GetElements(itms));

            base.Execute(
                new UnitTestElements(new TestAncestorCriterion(unitTestElements), unitTestElements.ToArray())
                , sessionDescriptor, solution, context);
        }
    }
     */
}
