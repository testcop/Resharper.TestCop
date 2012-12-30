using System;
using System.Collections.Generic;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using JetBrains.DocumentModel;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.FileTemplates;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.LiveTemplates.Templates;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Services;
using JetBrains.TextControl;
using JetBrains.Threading;
using JetBrains.UI;
using JetBrains.UI.PopupWindowManager;
using JetBrains.UI.RichText;
using JetBrains.UI.Tooltips;
using JetBrains.Util;
using System.Linq;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper
{
    static class ResharperHelper
    {
        public static void ForceKeyboardBindings()
        {
            const string macroName = "Resharper_TestCop_JumpToTest";
            const string keyboardShortcut = "Global::Ctrl+G, Ctrl+T";

            ExecuteActionOnUiThread("force TestCop keyboard shortcut hack",
                ()=>DTEHelper.AssignKeyboardShortcutIfMissing(macroName, keyboardShortcut) );        
        }

        public static void AppendLineToOutputWindow(string msg)
        {
            ExecuteActionOnUiThread("testCop append text to output pane",
                () => DTEHelper.GetOutputWindowPane("TestCop",false).OutputString(msg+"\n") );                    
        }

        public static Action ProtectActionFromReEntry(Lifetime lifetime, Action fOnExecute)
        {
            System.Action fOnExecute2 = (System.Action)(() => IThreadingEx.ExecuteOrQueue(
                (IThreading)Shell.Instance.Locks, lifetime, "TestingMenuNavigation", fOnExecute));
            return fOnExecute2;
        }

        public static string UsingFileNameGetClassName(string baseFileName)
        {
            if (baseFileName.Contains("."))
            {
                //handles Class.DataAccessTests
                string className = baseFileName.Split(new[] { '.' })[0];
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
                context.GetData(JetBrains.ProjectModel.DataContext.DataConstants.PROJECT_MODEL_ELEMENT);

            var projectItem = projectModelElement as IProjectItem;
            if (projectItem == null) return null;


            FileSystemPath location = projectItem.Location;
            string fileName = location.NameWithoutExtension;  
                     
            return fileName;                        
        }
        
        public static IClrTypeName FindFirstTypeInFile(ISolution solution, IDocument document)
        {           
            for (int i = document.DocumentRange.StartOffset; i < document.DocumentRange.EndOffset; i++)
            {
                var firstTypeInFile = TextControlToPsi.GetContainingTypeOrTypeMember(solution, document, i) as ITypeElement;
                
                if (firstTypeInFile != null)
                {
                    AppendLineToOutputWindow("Hunted and found first name in file to be " + firstTypeInFile.GetClrName());
                    return firstTypeInFile.GetClrName();
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

            AppendLineToOutputWindow("Element at cursor is of type " + documentElement.GetType().Name);

            IClrTypeName clrTypeName = null;

            if (documentElement is IClass)
            {
                clrTypeName = ((IClass)documentElement).GetClrName();
            }

            if (documentElement is ITypeElement && clrTypeName == null)
            {
                var containingType = ((ITypeElement)documentElement).GetContainingType();
                if (containingType != null) clrTypeName= containingType.GetClrName();                
            }
          
            if (clrTypeName == null)
            {
                AppendLineToOutputWindow("Unable to identify the class from current cursor position.");
                return FindFirstTypeInFile(solution, textControl.Document);
            }
            return clrTypeName;
        }
      
        public static void ShowTooltip(IDataContext context, ISolution solution, RichText tooltip)
        {
            var shellLocks = solution.GetComponent<IShellLocks>();
            var tooltipManager = solution.GetComponent<ITooltipManager>();

            tooltipManager.Show(tooltip,
              lifetime =>
              {
                  var windowContextSource = context.GetData(JetBrains.UI.DataConstants.PopupWindowContextSource);

                  if (windowContextSource != null)
                  {
                      var windowContext = windowContextSource.Create(lifetime);
                      var ctxTextControl = windowContext as TextControlPopupWindowContext;
                      return ctxTextControl == null ? windowContext :
                        ctxTextControl.OverrideLayouter(lifetime, lifetimeLayouter => new DockingLayouter(lifetimeLayouter, new TextControlAnchoringRect(lifetimeLayouter, ctxTextControl.TextControl, ctxTextControl.TextControl.Caret.Offset(), shellLocks), Anchoring2D.AnchorTopOrBottom));
                  }

                  return solution.GetComponent<MainWindowPopupWindowContext>().Create(lifetime);
              });
        }

        public static IProject FindAssociatedProject(IProject project)
        {
             ISolution solution = project.GetSolution();
            string currentProjectNamespace = project.GetDefaultNamespace();
            if (string.IsNullOrEmpty(currentProjectNamespace)) return null;

            IProject associatedProject;
            if (currentProjectNamespace.EndsWith(".Tests"))
            {
                associatedProject =
                    GetNonTestProjects(solution).SingleOrDefault(
                        p => p.GetDefaultNamespace() == currentProjectNamespace.RemoveTrailing(".Tests"));
            }
            else
            {
                associatedProject =
                    GetTestProjects(solution).SingleOrDefault(p => p.GetDefaultNamespace() == currentProjectNamespace + ".Tests");
            }
            return associatedProject;
        }

        public static IEnumerable<IProject> GetTestProjects(ISolution solution)
        {
            return solution.GetAllProjects().Where(x=>x.GetOutputAssemblyName().EndsWith(".Tests"));
        }

        public static void RemoveElementsNotInProject(List<IClrDeclaredElement> declaredElements, IProject associatedProject)
        {
            declaredElements.RemoveAll(p => p.GetSourceFiles().Any(de =>
            {
                var project = de.GetProject();
                return project != null &&
                       project != associatedProject;
            }));
        }

        public static IEnumerable<IProject> GetNonTestProjects(ISolution solution)
        {
            return solution.GetAllProjects().Where(x => !x.GetOutputAssemblyName().EndsWith(".Tests"));
        }

        public static List<IClrDeclaredElement> FindClass(ISolution solution, string classNameToFind, params IProject[] restrictToThisProjects)
        {
            IDeclarationsCache declarationsCache = solution.GetPsiServices().CacheManager.GetDeclarationsCache(DeclarationCacheLibraryScope.NONE, false);
            var results=declarationsCache.GetElementsByShortName(classNameToFind).ToList();

            foreach (var restrictToThisProject in restrictToThisProjects)
            {
                RemoveElementsNotInProject(results, restrictToThisProject);    
            }
            
            return results;
        }
               
        private static void ExecuteActionOnUiThread(string description, Action fOnExecute)
        {
            var threading = Shell.Instance.GetComponent<IThreading>();
            threading.ExecuteOrQueue(description, fOnExecute);                    
        }

        public static void CreateFileWithinProject(IDataContext context, IProject associatedProject,
                                                    FileSystemPath fileSystemPath, string targetFile)
        {
            Template classTemplate = FileTemplatesManager.Instance.GetFileTemplatesForActions(context).Where(c => c.Shortcut == "Class").SingleOrDefault();
            IProjectFolder folder = (IProjectFolder) associatedProject.FindProjectItemByLocation(fileSystemPath)
                                    ?? AddNewItemUtil.GetOrCreateProjectFolder(associatedProject, fileSystemPath);

            if(folder==null)
            {
                Logger.LogMessage(LoggingLevel.NORMAL, "Error failed to create/location project folder"+fileSystemPath);
                return;
            }
            IProjectFile newFile = FileTemplatesManager.Instance.CreateFileFromTemplate(targetFile+".cs", folder, classTemplate);            
        }
    }    
}
