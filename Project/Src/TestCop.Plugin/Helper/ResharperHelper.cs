﻿// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2018
// --

namespace TestCop.Plugin.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using JetBrains.Application.DataContext;
    using JetBrains.Application.Threading;
    using JetBrains.DocumentModel;
    using JetBrains.Lifetimes;
    using JetBrains.Metadata.Reader.API;
    using JetBrains.ProjectModel;
    using JetBrains.ProjectModel.DataContext;
    using JetBrains.ReSharper.Feature.Services.Util;
    using JetBrains.ReSharper.Psi;
    using JetBrains.ReSharper.Psi.Caches;
    using JetBrains.ReSharper.Psi.CSharp.Tree;
    using JetBrains.ReSharper.Psi.Util;
    using JetBrains.ReSharper.Resources.Shell;
    using JetBrains.TextControl;
    using JetBrains.Util;
    using JetBrains.Util.Logging;

    using TestCop.Plugin.Extensions;

    public static class ResharperHelper
    {
        public static string MacroNameSwitchBetweenFiles =>
            $"Resharper.ReSharper_{nameof(TestCopJumpToTestFileAction).RemoveTrailing("Action")}";

        public static string MacroNameRunTests => 
            $"ReSharper_{nameof(TestCopUnitTestRunContextAction).RemoveTrailing("Action")}";

        public static void PrintKeyboardBindings(IShellLocks shellLocks)
        {
            ExecuteActionOnUiThread(shellLocks, "Print TestCop keyboard shortcut",
                () =>
                {
                    if (DTEHelper.VisualStudioIsPresent())
                    {
                        DTEHelper.PrintoutKeyboardShortcut(
                            TestCopSettingsManager.Instance.Settings.OutputPanelOpenOnKeyboardMapping
                            , MacroNameSwitchBetweenFiles
                            , TestCopSettingsManager.Instance.Settings.ShortcutToSwitchBetweenFiles);

                        DTEHelper.PrintoutKeyboardShortcut(
                            TestCopSettingsManager.Instance.Settings.OutputPanelOpenOnKeyboardMapping
                            , MacroNameRunTests
                            , TestCopSettingsManager.Instance.Settings.ShortcutToRunTests);
                    }
                });
        }

        public static void ForceKeyboardBindings(IShellLocks shellLocks)
        {
            ExecuteActionOnUiThread(shellLocks, "force TestCop keyboard shortcut",
                () =>
                {
                    if (DTEHelper.VisualStudioIsPresent())
                    {
                        DTEHelper.AssignKeyboardShortcutIfMissing(
                            TestCopSettingsManager.Instance.Settings.OutputPanelOpenOnKeyboardMapping
                            , MacroNameSwitchBetweenFiles
                            , TestCopSettingsManager.Instance.Settings.ShortcutToSwitchBetweenFiles);

                        DTEHelper.AssignKeyboardShortcutIfMissing(
                            TestCopSettingsManager.Instance.Settings.OutputPanelOpenOnKeyboardMapping
                            , MacroNameRunTests
                            , TestCopSettingsManager.Instance.Settings.ShortcutToRunTests);
                    }
                });
        }

        public static void AppendLineToOutputWindow(IShellLocks shellLocks, string msg)
        {
            Logger.LogMessage(msg);
            ExecuteActionOnUiThread(shellLocks, "testCop append text to output pane",
                () =>
                {
                    if (DTEHelper.VisualStudioIsPresent())
                    {
                        DTEHelper.GetOutputWindowPane("TestCop", false).OutputString(msg + "\n");
                    }
                });
        }

        public static Action ProtectActionFromReEntry(Lifetime lifetime, string name, Action fOnExecute)
        {
            void fOnExecute2()
            {
                IShellLocks shellLocks = Shell.Instance.GetComponent<IShellLocks>();
                shellLocks.ExecuteOrQueue(lifetime, name, () => ReadLockCookie.Execute(fOnExecute));
            }

            return fOnExecute2;
        }

        public static string UsingFileNameGetClassName(string baseFileName)
        {
            const char splitChar = '.';

            if (baseFileName.Contains(splitChar))
            {
                string className = baseFileName.Split(splitChar)[0];
                return className;
            }

            return baseFileName;
        }

        public static string GetRelativeNameSpace(IProject project, IClrTypeName clrTypeClassName)
        {
            string targetNameSpace =
                clrTypeClassName.GetNamespaceName().RemoveLeading(project.GetDefaultNamespace()).RemoveLeading(".");

            return targetNameSpace;
        }

        public static string GetBaseFileName(IDataContext context, ISolution solution)
        {
            IProjectModelElement projectModelElement =
                context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT);

            if (!(projectModelElement is IProjectItem projectItem))
            {
                return null;
            }

            VirtualFileSystemPath location = projectItem.Location;
            string fileName = location.NameWithoutExtension;

            fileName = fileName.RemoveTrailing(".partial");

            return fileName;
        }

        public static IClrTypeName FindFirstTypeInFile(ISolution solution, IDocument document)
        {
            if (FindDeclaredElementInFile(solution, document, 1) is ITypeElement firstTypeInFile)
            {
                AppendLineToOutputWindow(solution.Locks,
                    "Hunted and found first name in file to be " + firstTypeInFile.GetClrName());
                return firstTypeInFile.GetClrName();
            }

            return null;
        }

        /*
        public static IEnumerable<ITypeElement> GetTypesInFile(IProjectFile projectFile)
        {
            var sourceFile = projectFile.ToSourceFile();
            if (sourceFile == null)
                return new List<ITypeElement>();
            
            var services = sourceFile.GetPsiServices();            
            return services.Symbols.GetTypesAndNamespacesInFile(sourceFile).OfType<ITypeElement>();
        }
        */
        public static IDeclaredElement FindDeclaredElementInFile(ISolution solution, IDocument document,
            int declarationSequencePosition)
        {
            List<string> typesFound = new List<string>();

            for (int i = document.DocumentRange.StartOffset; i < document.DocumentRange.EndOffset; i++)
            {
                IDeclaredElement typeInFile =
                    TextControlToPsi.GetContainingTypeOrTypeMember(solution, new DocumentOffset(document, i));

                if (typeInFile != null)
                {
                    if (!typesFound.Contains(typeInFile.ShortName))
                    {
                        typesFound.Add(typeInFile.ShortName);
                    }

                    if (typesFound.Count == declarationSequencePosition)
                    {
                        return typeInFile;
                    }
                }
            }

            return null;
        }

        public static ICSharpTypeDeclaration FindFirstCharpTypeDeclarationInDocument(ISolution solution, IDocument document)
        {
            for (int i = document.DocumentRange.StartOffset; i < document.DocumentRange.EndOffset; i++)
            {
                ICSharpTypeDeclaration declaration = TextControlToPsi
                    .GetElements<ICSharpTypeDeclaration>(solution, new DocumentOffset(document, i)).FirstOrDefault();

                if (declaration != null)
                {
                    return declaration;
                }
            }

            return null;
        }

        public static IClrTypeName GetClassNameAppropriateToLocation(ISolution solution, ITextControl textControl)
        {
            IDeclaredElement documentElement = TextControlToPsi.GetContainingTypeOrTypeMember(solution, textControl);

            if (documentElement == null)
            {
                return FindFirstTypeInFile(solution, textControl.Document);
            }

            //AppendLineToOutputWindow("Element at cursor is of type " + documentElement.GetType().Name);

            IClrTypeName clrTypeName = null;

            if (documentElement is IClass @class)
            {
                clrTypeName = @class.GetClrName();
            }

            if (documentElement is ITypeElement element && clrTypeName == null)
            {
                ITypeElement containingType = element.GetContainingType();

                if (containingType != null)
                {
                    clrTypeName = containingType.GetClrName();
                }
            }

            if (clrTypeName == null)
            {
                AppendLineToOutputWindow(solution.Locks, "Unable to identify the class from current cursor position.");
                return FindFirstTypeInFile(solution, textControl.Document);
            }

            return clrTypeName;
        }

        /*
        public static void ShowTooltip(IDataContext context, ISolution solution, RichText tooltip)
        {
            var tooltipManager = solution.GetComponent<ITooltipManager>();

            var windowContextSource = context.GetData<PopupWindowContextSource>(UIDataConstants.PopupWindowContextSource)
                                      ?? solution.GetComponent<IMainWindowPopupWindowContext>().Source;

            tooltipManager.Show(tooltip, windowContextSource);
        }
        */
        public static void RemoveElementsNotInProjects(List<IClrDeclaredElement> declaredElements,
            IList<IProject> associatedProjects)
        {
            declaredElements.RemoveAll(p => p.GetSourceFiles().Any(de =>
            {
                IProject project = de.GetProject();
                return project != null && associatedProjects.Contains(project) == false;
            }));
        }

        public static List<IClrDeclaredElement> FindClass(ISolution solution, string classNameToFind)
        {
            List<IProject> codeProjects = solution.GetAllCodeProjects().ToList();
            return FindClass(solution, classNameToFind, codeProjects);
        }

        public static List<IClrDeclaredElement> FindClass(ISolution solution, string classNameToFind,
            IProject restrictToThisProject)
        {
            return FindClass(solution, classNameToFind, new[] { restrictToThisProject });
        }

        public static List<IClrDeclaredElement> FindClass(ISolution solution, string classNameToFind,
            IList<TestCopProjectItem> restrictToTheseProjects)
        {
            return FindClass(solution, classNameToFind, restrictToTheseProjects.ToList(p => p.Project));
        }

        public static List<IClrDeclaredElement> FindClass(ISolution solution, string classNameToFind,
            IList<IProject> restrictToTheseProjects)
        {
            ISymbolScope declarationsCache = solution.GetPsiServices().Symbols
                .GetSymbolScope(LibrarySymbolScope.FULL, false); //, currentProject.GetResolveContext());

            List<IClrDeclaredElement> results = declarationsCache.GetElementsByShortName(classNameToFind).ToList();

            RemoveElementsNotInProjects(results, restrictToTheseProjects);

            return results;
        }

        public static IEnumerable<ITypeElement> FindFirstTypeWithinCodeFiles(ISolution solution, Regex regex, IProject project)
        {
            List<ProjectFileFinder.Match> items = new List<ProjectFileFinder.Match>();
            project.Accept(new ProjectFileFinder(items, regex));

            return items
                .SelectMany(p => solution.GetPsiServices().Symbols.GetTypesAndNamespacesInFile(p.ProjectFile.ToSourceFile()))
                .OfType<ITypeElement>();
        }

        private static void ExecuteActionOnUiThread(IThreading shellLocks, string description, Action fOnExecute)
        {
            shellLocks.ExecuteOrQueueEx(description, fOnExecute);
        }

        public static void CreateFileWithinProject(TestCopProjectItem projectItem, string targetFile)
        {
            TestCopFileCreator testCopFileCreator = Shell.Instance.GetComponent<TestCopFileCreator>();
            testCopFileCreator.CreateFileWithinProject(projectItem, targetFile);
        }
    }
}