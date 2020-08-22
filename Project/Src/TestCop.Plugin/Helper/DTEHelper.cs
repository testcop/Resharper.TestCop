// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2020
// --

using System;
using EnvDTE;
using JetBrains;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util.Logging;

namespace TestCop.Plugin.Helper
{
    public static class DTEHelper
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
        public static void PrintoutKeyboardShortcut(bool showOutputPane, string macroName, string keyboardShortcut)
        {                   
            var dte = Shell.Instance.GetComponent<DTE>();

            try
            {
                var command = dte.Commands.Item(macroName);

                if (command != null)
                {
                    var currentBindings = (System.Object[]) command.Bindings;

                    if (currentBindings.Length > 0)
                    {
                        //if (currentBindings[0].ToString() == keyboardShortcut)
                        for (int i = 0; i < currentBindings.Length; i++)
                        {
                            GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString(
                                $"TestCop keyboard shortcut for '{macroName}' is already set to '{currentBindings[i]}'\n");
                        }
                        return;
                    }
                    GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString(
                        $"Within Visual Studio - please map testCop keyboard shortcut for '{macroName}' to '{keyboardShortcut}'\n");

                }
            }
            catch (Exception e)
            {
                GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString(
                    "Error on setting '{0}' to '{1}. Ex={2}'\n".FormatEx(macroName, keyboardShortcut, e.ToString()));
                Logger.LogException(e);
            }
        }

        /// <summary>
        /// Must run on main UI thread
        /// </summary>        
        public static void AssignKeyboardShortcutIfMissing(bool showOutputPane, string macroName, string keyboardShortcut)
        {
            var dte = Shell.Instance.GetComponent<DTE>();

            try
            {
                var command = dte.Commands.Item(macroName);

                if (command != null)
                {
                    var currentBindings = (System.Object[])command.Bindings;

                    if (currentBindings.Length > 0)
                    {
                        for (int i = 0; i < currentBindings.Length; i++)
                        {
                            Logger.LogMessage($"Note TestCop keyboard shortcut for '{macroName}' is already set to '{currentBindings[i]}'\n");

                            GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString(
                                $"Note TestCop keyboard shortcut for '{macroName}' is already set to '{currentBindings[i]}'\n");
                        }
                        return;
                    }
                    
                    command.Bindings = string.IsNullOrEmpty(keyboardShortcut)
                        ? new Object[] {}
                        : new Object[] {keyboardShortcut};

                    Logger.LogMessage($"TestCop is setting keyboard shortcut for '{macroName}' to '{keyboardShortcut}'\n");

                    GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString(
                        $"TestCop is setting keyboard shortcut for '{macroName}' to '{keyboardShortcut}'\n"
                    );
                }
            }
            catch (Exception e)
            {
                GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString(
                    "Error on setting '{0}' to '{1}. Ex={2}'\n".FormatEx(macroName, keyboardShortcut, e.ToString()));
                Logger.LogException(e);
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
