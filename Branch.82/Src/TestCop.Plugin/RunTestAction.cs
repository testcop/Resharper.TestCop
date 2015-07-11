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
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.UI.PopupMenu;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;

namespace TestCop.Plugin
{
    [ActionHandler("Testcop.UnitTestRunForContext")]
    public class TestCopUnitTestRunContextAction : UnitTestRunContextActionBase
    {
        protected override bool Update(ActionPresentation presentation, bool hasAnyTests, IUnitTestSessionView session)
        {
            var resut=base.Update(presentation, hasAnyTests, session);
            return resut;
        }

        protected override void Execute(UnitTestElements elements, IUnitTestSessionView session, ISolution solution, IDataContext context)
        {
            if (elements != null && elements.Elements.Count != 0)
            {
                //default behaviour if tests are present
                base.Execute(elements, session, solution, context);
                return;
            }


            JetPopupMenu menuAction=null;

            //Not the most elegant approach but avoids code duplication by using the main TestCop action
            var switchAction = (IActionHandler) new JumpToTestFileAction((menu, when) => menuAction=menu);
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

            base.Execute(new UnitTestElements(unitTestElements, null), session, solution, context);
        }
    }
}
