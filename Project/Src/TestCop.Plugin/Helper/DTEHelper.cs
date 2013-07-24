// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
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

        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        public static void AssignKeyboardShortcutIfMissing(string macroName, string keyboardShortcut, string replaceIfThisKeyboardShortcut)
        {            
            var dte = Shell.Instance.GetComponent<DTE>();
            
            var command = dte.Commands.Item(macroName);

            if (command != null)
            {
                var currentBindings = (System.Object[]) command.Bindings;

                if (currentBindings.Length == 1 && !string.IsNullOrEmpty(replaceIfThisKeyboardShortcut))
                {
                    if (currentBindings[0].ToString() != replaceIfThisKeyboardShortcut)
                    {
                        //already mapped and doesn't need to be overidden
                        return;
                    }
                }

                if (currentBindings.Length == 1 && string.IsNullOrEmpty(replaceIfThisKeyboardShortcut))
                {           
                    //already mapped
                    return;                   
                }

                command.Bindings = string.IsNullOrEmpty(keyboardShortcut)
                                        ? new Object[] {}
                                        : new Object[] {keyboardShortcut};
                GetOutputWindowPane(dte, "TestCop", true).OutputString(
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
