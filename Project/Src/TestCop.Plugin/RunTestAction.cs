// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2015
// --
using System.Collections.Generic;
using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Actions;
using JetBrains.ReSharper.UnitTestFramework.Criteria;
using JetBrains.UI.ActionsRevised;
using JetBrains.UI.PopupMenu;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin
{    
    [Action("Testcop Run Unit Tests", Id = 92407+1, ShortcutScope = ShortcutScope.TextEditor, Icon = typeof(UnnamedThemedIcons.Agent16x16))]
    public class TestCopUnitTestRunContextAction : UnitTestRunFromContextAction
    {
        protected override IHostProvider GetHostProvider()
        {
            return UnitTestHost.Instance.GetProvider("Process");
        }

        protected override UnitTestElements GetElementsToRun(IDataContext context)
        {            
            var elements = base.GetElementsToRun(context);
            if (elements != null && elements.Explicit.Count != 0)
            {
                //Default Test Behaviour
                return elements;
            }

            return GetElementsToRunViaTestCop(context);
        }

        public UnitTestElements GetElementsToRunViaTestCop(IDataContext context)
        {
             JetPopupMenu menuAction=null;

            //Not the most elegant approach but avoids code duplication by using the main TestCop action
            var switchAction = (IExecutableAction) JumpToTestFileAction.CreateWith((menus, menu, when) => menuAction=menu);
            switchAction.Execute(context, null);

            if (menuAction == null) 
            {
                return null;
            }
            
            var itms = menuAction.ItemKeys.Where(i => i is SimpleMenuItemForProjectItem) //only links to project files
                .Cast<SimpleMenuItemForProjectItem>()
                .Where(i => i.AssociatedProjectItem.GetProject().IsTestProject())//only files within test projects
                .Select(i => i.DeclaredElement) 
                .ToList();

            if (itms.Count == 0)
            {
                return null;
            }
            
            var mgr = context.GetComponent<IUnitTestElementStuff>();
            var unitTestElements = new List<IUnitTestElement>();

            unitTestElements.AddRange(mgr.GetElements(itms));

           return new UnitTestElements(new TestAncestorCriterion(unitTestElements), unitTestElements.ToArray());          
        }       
    }     
}
