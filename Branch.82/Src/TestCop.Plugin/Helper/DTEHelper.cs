// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System;
using EnvDTE;
using JetBrains.Application;

namespace TestCop.Plugin.Helper
{
    static public class DTEHelper
    {
        public static bool VisualStudioIsPresent()
        {            
            return Shell.Instance.HasComponent<DTE>();
        }

        public static void RefreshSolutionExplorerWindow()
        {
            var dte = Shell.Instance.GetComponent<DTE>();            
            dte.Commands.Raise("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}", 222, null, null);
        }
   
        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        public static void AssignKeyboardShortcutIfMissing(bool showOutputPane, string macroName, string keyboardShortcut)
        {            
            var dte = Shell.Instance.GetComponent<DTE>();
            
            var command = dte.Commands.Item(macroName);

            if (command != null)
            {
                var currentBindings = (System.Object[]) command.Bindings;
             
                if (currentBindings.Length == 1)
                {
                    if (currentBindings[0].ToString() == keyboardShortcut)
                    {
                        GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString(
                            string.Format("Keyboard shortcut for '{0}' is '{1}'\n", macroName, keyboardShortcut));
                        return;
                    }
                }

                command.Bindings = string.IsNullOrEmpty(keyboardShortcut)
                                        ? new Object[] {}
                                        : new Object[] {keyboardShortcut};
                GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString(
                    string.Format("Setting keyboard shortcut for '{0}' to '{1}'\n", macroName, keyboardShortcut)
                    );      
            }
        }
        
        public static OutputWindowPane GetOutputWindowPane(string name, bool show)
        {               
            var dte = Shell.Instance.GetComponent<DTE>();
            return GetOutputWindowPane(dte, name, show);
        }      

        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        private static OutputWindowPane GetOutputWindowPane(DTE dte, string name, bool show)
        {
            /* If compilation generates:: 'EnvDTE.Constants' can be used only as one of its applicable interfaces
             * then set DTE assembly reference property Embed Interop Types = false  */
            
            var win = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);            
            if(show)win.Visible = true;

            var ow = (OutputWindow) win.Object;
            OutputWindowPane owpane;
            try
            {
                owpane = ow.OutputWindowPanes.Item(name);
            }
            catch(Exception)            
            {
                owpane = ow.OutputWindowPanes.Add(name);
            }

            owpane.Activate();
            return owpane;
        }        
    }

}
