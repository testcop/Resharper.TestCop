// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.CommonControls;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.DocumentModel;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Navigation;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.UI.PopupMenu;
using JetBrains.UI.RichText;
using JetBrains.Util;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper
{
    public static class JumpToTestMenuHelper
    {        
        //------------------------------------------------------------------------------------------------------------------------
        public static void PromptToOpenOrCreateClassFiles(Action<JetPopupMenu, JetPopupMenu.ShowWhen> menuDisplayer,Lifetime lifetime, IDataContext context, ISolution solution
    , IProject project, IClrTypeName clrTypeClassName, IList<TestCopProjectItem> targetProjects
    , List<IClrDeclaredElement> preferred, List<IClrDeclaredElement> fullList)
        {
            var autoExecuteIfSingleEnabledItem = JetPopupMenu.ShowWhen.AutoExecuteIfSingleEnabledItem;
            var menuItems = new List<SimpleMenuItem>();

            if (preferred.Count > 0)
            {
                AppendNavigateToMenuItems(lifetime, solution, preferred, menuItems);
            }
            else
            {
                AppendNavigateToMenuItems(lifetime, solution, fullList, menuItems);                
            }

            MoveBestMatchesToTopWhenSwitchingFromTestToCode(menuItems, project, targetProjects, clrTypeClassName);

            if (clrTypeClassName != null)
            {
                if (DeriveRelatedFileNameAndAddCreateMenus(context, lifetime, project, targetProjects, menuItems, clrTypeClassName))
                {
                    autoExecuteIfSingleEnabledItem = JetPopupMenu.ShowWhen.NoItemsBannerIfNoItems;
                }
            }
             
            var menu = Shell.Instance.GetComponent<JetPopupMenus>().Create();
            menu.Caption.Value = WindowlessControl.Create("Switch to:");
            menu.SetItems(menuItems.ToArray());

            PositionPopMenuCorrectly(context, lifetime, menu);

            menu.KeyboardAcceleration.SetValue(KeyboardAccelerationFlags.Mnemonics);
            menu.NoItemsBanner = WindowlessControl.Create("No destinations found.");
     
            menuDisplayer.Invoke(menu, autoExecuteIfSingleEnabledItem);                        
        }
        //------------------------------------------------------------------------------------------------------------------------
        private static void AppendNavigateToMenuItems(Lifetime lifetime, ISolution solution, List<IClrDeclaredElement> clrDeclaredElements,
                                                      List<SimpleMenuItem> menuItems)
        {
            foreach (var declaredElement in clrDeclaredElements)
            {
                var simpleMenuItems = DescribeFilesAssociatedWithDeclaredElement(lifetime, DocumentManager.GetInstance(solution),
                                                                                 declaredElement
                                                                                 ,
                                                                                 p =>
                                                                                 () =>
                                                                                 EditorManager.GetInstance(solution).
                                                                                     OpenProjectFile(p, true)
                    );
                menuItems.AddRange(simpleMenuItems);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------
        private static void PositionPopMenuCorrectly(IDataContext context, Lifetime lifetime, JetPopupMenu menu)
        {
            var windowContextSource = context.GetData(JetBrains.UI.DataConstants.PopupWindowContextSource);

            if (windowContextSource != null)
            {
                var windowContext = windowContextSource.Create(lifetime);
                menu.PopupWindowContext = windowContext;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------
        private static IList<SimpleMenuItem> DescribeFilesAssociatedWithDeclaredElement(Lifetime lifetime, DocumentManager documentManager, IClrDeclaredElement declaredElement, Func<IProjectFile, Action> clickAction)
        {
            IList<SimpleMenuItem> menuItems = new List<SimpleMenuItem>();            
            //var iconManager = SolutionEx.GetComponent<PsiIconManager>(solution);
            //, iconManager.GetImage(declaredElement.GetElementType())
            var projectFiles = GetProjectFiles(documentManager, declaredElement);

            foreach (var projectFile in projectFiles)
            {
                IProjectFile currentProjectFile = projectFile;
                var np = new ProjectFileNavigationPoint(currentProjectFile);

                var result = new SimpleMenuItemForProjectItem(np.GetPresentationText(), np.GetPresentationImage()
                                                , ResharperHelper.ProtectActionFromReEntry(lifetime,"TestingMenuNavigation", clickAction.Invoke(projectFile))
                                                ,projectFile)
                {
                    ShortcutText = np.GetSecondaryPresentationText(),
                    Style = MenuItemStyle.Enabled,
                    Tag = projectFile.Location.FullPath
                };

                menuItems.Add(result);
            }
            return menuItems;
        }

        static public void MoveBestMatchesToTopWhenSwitchingFromTestToCode(IList<SimpleMenuItem> currentMenus
            , IProject project
            , IList<TestCopProjectItem> associatedTargetProjects
            , IClrTypeName clrTypeClassName)
        {
            if (clrTypeClassName == null) return;
//            if (!project.IsTestProject()) return;

            foreach (string testSuffix in TestCopSettingsManager.Instance.Settings.TestClassSuffixes())
            {
                bool currentFileisTestFile = clrTypeClassName.ShortName.EndsWith(testSuffix);
                string targetFileName = clrTypeClassName.ShortName.Flip(currentFileisTestFile, testSuffix);

                foreach (var associatedTargetProject in associatedTargetProjects)
                {
                    var targetFilePathName =
                        FileSystemPath.Parse(associatedTargetProject.SubNamespaceFolder + "\\" + targetFileName);

                    for (int i = 0; i < currentMenus.Count; i++)
                    {
                        var menuItem = currentMenus[i];
                        if (menuItem.Tag == null) continue;

                        if (menuItem.Tag.ToString()
                            .StartsWith(targetFilePathName.FullPath, StringComparison.CurrentCultureIgnoreCase))
                        {
                            currentMenus.RemoveAt(i);
                            currentMenus.Insert(0, menuItem);
                        }
                    }
                }
            }
        }

   
        //------------------------------------------------------------------------------------------------------------------------
        public static bool DeriveRelatedFileNameAndAddCreateMenus(IDataContext context, Lifetime lifetime,
            IProject project, IList<TestCopProjectItem> associatedTargetProjects, IList<SimpleMenuItem> currentMenus,
            IClrTypeName clrTypeClassName)
        {
            bool addedCreateMenuItem = false;

            if (clrTypeClassName == null) return false;
            var baseFileName = ResharperHelper.GetBaseFileName(context, project.GetSolution());

            var settings = TestCopSettingsManager.Instance.Settings;
            bool currentFileisTestFile = baseFileName.EndsWith(settings.TestClassSuffixes());

            foreach (var testClassSuffix in settings.GetAppropriateTestClassSuffixes(baseFileName))
            {                
                var targetFile = ResharperHelper.UsingFileNameGetClassName(baseFileName).RemoveTrailing(testClassSuffix);
                
                if (!currentFileisTestFile)
                {
                    targetFile += testClassSuffix;
                }

                foreach (var associatedTargetProject in associatedTargetProjects)
                {
                    if (currentFileisTestFile == associatedTargetProject.Project.IsTestProject())
                    {
                        ResharperHelper.AppendLineToOutputWindow(
                            string.Format("Internal Error: Attempted to create '{0}' within project '{1}'"
                                , targetFile, associatedTargetProject.Project.Name));
                        continue;
                    }

                    string targetFileLocation = associatedTargetProject.SubNamespaceFolder.FullPath + "\\" + targetFile;

                    if (!IsMenuItemPresentForFile(currentMenus, targetFileLocation))
                    {
                        currentMenus.AddRange(AddCreateFileMenuItem(lifetime, associatedTargetProject.Project,
                            associatedTargetProject.SubNamespaceFolder, targetFile));
                        addedCreateMenuItem = true;
                    }
                }
            }

            return addedCreateMenuItem;
        }

        private static bool IsMenuItemPresentForFile(IList<SimpleMenuItem> currentMenus, string targetFileLocation)
        {
            foreach (var menuItem in currentMenus)
            {
                if (menuItem.Tag == null) continue;
                if (menuItem.Tag.ToString().StartsWith(targetFileLocation, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        //------------------------------------------------------------------------------------------------------------------------
        private static List<SimpleMenuItem> AddCreateFileMenuItem(Lifetime lifetime, IProject associatedTargetProject,
                                          FileSystemPath targetLocationDirectory, string targetFile)
        {
            var menuItems = new List<SimpleMenuItem>();

            var result = new SimpleMenuItem("Create associated file"
                                            , null,
                                            ResharperHelper.ProtectActionFromReEntry(lifetime,"TestingMenuNavigation",
                                                                     () =>
                                                                     ResharperHelper.CreateFileWithinProject(associatedTargetProject,
                                                                                             targetLocationDirectory, targetFile)));
            result.Style = MenuItemStyle.Enabled;
            result.Icon = UnnamedThemedIcons.Agent16x16.Id;
            result.Text = new RichText("Create ", TextStyle.FromForeColor(Color.Green)).Append(targetFile, TextStyle.FromForeColor(TextStyle.DefaultForegroundColor));
            result.ShortcutText = new RichText("(" + associatedTargetProject.GetPresentableProjectPath()
                                               +
                                               targetLocationDirectory.FullPath.RemoveLeading(
                                                   associatedTargetProject.ProjectFileLocation.Directory.FullPath) + ")",
                                               TextStyle.FromForeColor(Color.LightGray));
            menuItems.Add(result);
            return menuItems;
        }        
        //------------------------------------------------------------------------------------------------------------------------
        private static IList<IProjectFile> GetProjectFiles(DocumentManager documentManager, IDeclaredElement declaredElement)
        {
            IList<IProjectFile> results = new List<IProjectFile>();
            foreach (var declaration in declaredElement.GetDeclarations())
            {
                DocumentRange documentRange = declaration.GetNavigationRange();
                if (!documentRange.IsValid())
                    documentRange = TreeNodeExtensions.GetDocumentRange(declaration);

                if (documentRange.IsValid())
                {
                    IProjectFile projectFile = documentManager.TryGetProjectFile(documentRange.Document);
                    if (projectFile != null)
                    {
                        results.Add(projectFile);
                    }
                }
            }
            return results;
        }
        //------------------------------------------------------------------------------------------------------------------------
    }
}
