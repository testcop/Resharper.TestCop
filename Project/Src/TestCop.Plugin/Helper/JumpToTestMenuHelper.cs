using System;
using System.Collections.Generic;
using System.Drawing;
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
        public static void PromptToOpenOrCreateClassFiles(Lifetime lifetime, IDataContext context, ISolution solution
    , IProject project, IClrTypeName clrTypeClassName, IProject targetProject
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

            if (clrTypeClassName != null)
            {                
                if(DeriveRelatedFileNameAndAddCreateMenus(context, lifetime, project, targetProject,menuItems, clrTypeClassName))
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
            menu.Show(autoExecuteIfSingleEnabledItem);
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

                var result = new SimpleMenuItem(np.GetPresentationText(), np.GetPresentationImage()
                                                , ResharperHelper.ProtectActionFromReEntry(lifetime, clickAction.Invoke(projectFile)));

                result.ShortcutText = np.GetSecondaryPresentationText();
                result.Style = MenuItemStyle.Enabled;
                result.Tag = projectFile.Location.FullPath;

                menuItems.Add(result);
            }
            return menuItems;
        }
        //------------------------------------------------------------------------------------------------------------------------
        static public bool DeriveRelatedFileNameAndAddCreateMenus(IDataContext context, Lifetime lifetime, IProject project, IProject associatedTargetProject,IList<SimpleMenuItem> currentMenus, IClrTypeName clrTypeClassName)
        {
            const string testSuffix = "Tests";
            if (clrTypeClassName == null) return false;

            string targetNameSpace = ResharperHelper.GetRelativeNameSpace(project, clrTypeClassName);
            string targetDirectory = targetNameSpace.Replace('.', '\\');
                        
            var baseFileName = ResharperHelper.GetBaseFileName(context, project.GetSolution());
            
            var targetFile = ResharperHelper.UsingFileNameGetClassName(baseFileName).RemoveTrailing(testSuffix);
            bool isTestFile = baseFileName.EndsWith(testSuffix);

            if (!isTestFile)         
            {
                targetFile += testSuffix;
            }
            
            var targetLocationDirectory = new FileSystemPath(associatedTargetProject.ProjectFileLocation.Directory + "\\" + targetDirectory);

            foreach (var menuItem in currentMenus)
            {
                if (menuItem.Tag==null) continue;
                if(menuItem.Tag.ToString().StartsWith(targetLocationDirectory.FullPath+"\\"+targetFile, StringComparison.CurrentCultureIgnoreCase))
                {                    
                    return false;
                }
            }

            currentMenus.AddRange(AddCreateFileMenuItem(context, lifetime, associatedTargetProject, targetLocationDirectory, targetFile));
            return true;
        }
        //------------------------------------------------------------------------------------------------------------------------
        private static List<SimpleMenuItem> AddCreateFileMenuItem(IDataContext context, Lifetime lifetime, IProject associatedTargetProject,
                                          FileSystemPath targetLocationDirectory, string targetFile)
        {
            var menuItems = new List<SimpleMenuItem>();

            var result = new SimpleMenuItem("Create associated file"
                                            , null,
                                            ResharperHelper.ProtectActionFromReEntry(lifetime,
                                                                     () =>
                                                                     ResharperHelper.CreateFileWithinProject(context, associatedTargetProject,
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
                    documentRange = TreeNodeExtensions.GetDocumentRange((ITreeNode)declaration);

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
