// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2020
// --

namespace TestCop.Plugin.Helper
{
    using System;

    using EnvDTE;

    using EnvDTE80;

    using JetBrains;
    using JetBrains.ReSharper.Resources.Shell;
    using JetBrains.Util.Logging;

    public static class DTEHelper
    {
        public static bool VisualStudioIsPresent()
        {
            return Shell.Instance.HasComponent<DTE2>();
        }

        public static void RefreshSolutionExplorerWindow()
        {
            DTE2 dte = Shell.Instance.GetComponent<DTE2>();
            dte.Commands.Raise("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}", 222, null, null);
        }

        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        public static void PrintoutKeyboardShortcut(bool showOutputPane, string macroName, string keyboardShortcut)
        {
            DTE2 dte = Shell.Instance.GetComponent<DTE2>();

            try
            {
                Command command = dte.Commands.Item(macroName);

                if (command != null)
                {
                    object[] currentBindings = (object[])command.Bindings;

                    if (currentBindings.Length > 0)
                    {
                        //if (currentBindings[0].ToString() == keyboardShortcut)
                        foreach (object t in currentBindings)
                        {
                            GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString(
                                $"TestCop keyboard shortcut for '{macroName}' is set to '{t}'\n");
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
                    "Error on setting '{0}' to '{1}. Ex={2}'\n".FormatEx(macroName, keyboardShortcut, e));
                Logger.LogException(e);
            }
        }

        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        public static void AssignKeyboardShortcutIfMissing(bool showOutputPane, string macroName, string keyboardShortcut)
        {
            DTE2 dte = Shell.Instance.GetComponent<DTE2>();

            try
            {
                Command command = dte.Commands.Item(macroName);

                if (command != null)
                {
                    object[] currentBindings = (object[])command.Bindings;

                    if (currentBindings.Length > 0)
                    {
                        foreach (object t in currentBindings)
                        {
                            Logger.LogMessage(
                                $"Note that the TestCop keyboard shortcut for '{macroName}' is already set to '{t}'\n");

                            //GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString($"TestCop keyboard shortcut for '{macroName}' is already set to '{currentBindings[i]}'\n");
                        }

                        return;
                    }

                    command.Bindings = string.IsNullOrEmpty(keyboardShortcut)
                        ? new object[] { }
                        : new object[] { keyboardShortcut };

                    Logger.LogMessage($"TestCop is setting keyboard shortcut for '{macroName}' to '{keyboardShortcut}'\n");

                    GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString(
                        $"TestCop is setting keyboard shortcut for '{macroName}' to '{keyboardShortcut}'\n"
                    );
                }
            }
            catch (Exception e)
            {
                GetOutputWindowPane(dte, "TestCop", showOutputPane).OutputString(
                    "Error on setting '{0}' to '{1}. Ex={2}'\n".FormatEx(macroName, keyboardShortcut, e));
                Logger.LogException(e);
            }
        }

        public static OutputWindowPane GetOutputWindowPane(string name, bool show)
        {
            DTE2 dte = Shell.Instance.GetComponent<DTE2>();
            return GetOutputWindowPane(dte, name, show);
        }

        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        private static OutputWindowPane GetOutputWindowPane(DTE2 dte, string name, bool show)
        {
            /* If compilation generates:: 'EnvDTE.Constants' can be used only as one of its applicable interfaces
             * then set DTE assembly reference property Embed Interop Types = false  */

            Window win = dte.Windows.Item(Constants.vsWindowKindOutput);

            if (show)
            {
                win.Visible = true;
            }

            OutputWindow ow = (OutputWindow)win.Object;
            OutputWindowPane owpane;

            try
            {
                owpane = ow.OutputWindowPanes.Item(name);
            }
            catch (Exception)
            {
                owpane = ow.OutputWindowPanes.Add(name);
            }

            owpane.Activate();
            return owpane;
        }
    }
}