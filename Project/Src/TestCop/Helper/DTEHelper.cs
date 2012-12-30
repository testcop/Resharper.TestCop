using System;
using EnvDTE;
using JetBrains.Application;

namespace TestCop.Plugin.Helper
{
    static public class DTEHelper
    {
        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        public static void AssignKeyboardShortcutIfMissing(string macroName, string keyboardShortcut)
        {
            var dte = Shell.Instance.GetComponent<DTE>();            
            var command = dte.Commands.Item(macroName);
            var currentBindings = (System.Object[])command.Bindings;
            if (currentBindings.Length == 0)
            {
                command.Bindings = keyboardShortcut;
                GetOutputWindowPane(dte, "TestCop", true).OutputString(
                    string.Format("Setting keyboard shortcut for '{0}' to '{1}'\n",macroName,keyboardShortcut)
                    );
            }            
        }

        public static OutputWindowPane GetOutputWindowPane( string name, bool show)
        {
            var dte = Shell.Instance.GetComponent<DTE>();
            return GetOutputWindowPane(dte, name, show);
        }      

        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        public static OutputWindowPane GetOutputWindowPane(DTE dte, string name, bool show)
        {
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
