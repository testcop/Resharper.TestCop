// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2020
// --

namespace TestCop.Plugin.Helper
{
    using JetBrains;
    using JetBrains.ReSharper.Resources.Shell;
    using JetBrains.Util.Logging;
    using JetBrains.VsIntegration.Interop.Declarations;
    using JetBrains.VsIntegration.Shell.EnvDte;

    using System;

    using JetBrains.Platform.VisualStudio.Protocol.TemporarilyExposedToFront;

    public static class DTEHelper
    {
        public static bool VisualStudioIsPresent()
        {
            return Shell.Instance.HasComponent<IEnvDteWrapper>();
        }

        public static void RefreshSolutionExplorerWindow()
        {
            IEnvDteWrapper dte = Shell.Instance.GetComponent<IEnvDteWrapper>();
            dte.Commands.Raise(VsConstants.VSStd2K.ToString(), (int)VsKnownCommands.VSStd2KCmdID.SLNREFRESH, null, null);
        }

        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        public static void PrintoutKeyboardShortcut(bool showOutputPane, string macroName, string keyboardShortcut)
        {
            IEnvDteWrapper dte = Shell.Instance.GetComponent<IEnvDteWrapper>();

            try
            {
                IEnvDteCommand command = GetCommand(dte, macroName);

                if (command != null)
                {
                    object[] currentBindings = command.Bindings;

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
            IEnvDteWrapper dte = Shell.Instance.GetComponent<IEnvDteWrapper>();

            try
            {
                IEnvDteCommand command = GetCommand(dte, macroName);

                if (command != null)
                {
                    object[] currentBindings = command.Bindings;

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

        public static IEnvDteOutputWindowPane GetOutputWindowPane(string name, bool show)
        {
            IEnvDteWrapper dte = Shell.Instance.GetComponent<IEnvDteWrapper>();
            return GetOutputWindowPane(dte, name, show);
        }

        private static IEnvDteCommand GetCommand(IEnvDteWrapper dte, string macroName)
        {
            dte.Commands.CommandInfo(macroName, out string cmdGuid, out int cmdId);

            IEnvDteCommand command = dte.Commands.TryGetCommand(cmdGuid, cmdId);
            return command;
        }

        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        private static IEnvDteOutputWindowPane GetOutputWindowPane(IEnvDteWrapper dte, string name, bool show)
        {
            IEnvDteWindow window = dte.Windows.TryGetWindow(VsConstants.StandardToolWindows.Output.ToString());

            if (show)
            {
                window.Visible = true;
            }
            
            IEnvDteOutputWindow outputWindow = (IEnvDteOutputWindow)window.Object;
            IEnvDteOutputWindowPane outputWindowPane = outputWindow.OutputWindowPanes.TryGetPane(name) ?? outputWindow.OutputWindowPanes.Add(name);
            
            
            outputWindowPane.Activate();
            return outputWindowPane;
        }
    }
}