// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Refactorings.Rename;
using JetBrains.Util;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;

namespace TestCop.Plugin
{
    [FileRenameProvider]
    public class RenameTestFilesTooRefactoring : IFileRenameProvider 
    {        
        public IEnumerable<FileRename> GetFileRenames(IDeclaredElement declaredElement, string newName)
        {
            if (!Settings.SupportRenameRefactor) yield break;

            var typeElement = declaredElement as ITypeElement;                                 

            var clrDeclaredElement = declaredElement as IClrDeclaredElement;
            if (clrDeclaredElement == null) yield break;
            var psiModule = clrDeclaredElement.Module as IProjectPsiModule;
            if (psiModule == null) yield break;

            if (typeElement != null)
            {
                var classNameBeingRenamed = typeElement.GetClrName().ShortName; 
                var project = psiModule.Project;
                var solution = project.GetSolution();

                if (!project.IsTestProject())
                {
                    //get associated projects..
                    var targetProjects = project.GetAssociatedProjects(typeElement.GetClrName().GetNamespaceName());
                    if (targetProjects.IsEmpty())
                    {
                        yield break;
                    }
                    //now look for expected file names that are also in correct locations
                    var settings = TestCopSettingsManager.Instance.Settings;
                  
                    foreach (var testClassSuffix in settings.GetAppropriateTestClassSuffixes(psiModule.SourceFiles.First().Name))
                    {
                        var projectFilesWithMatchingName = new List<IProjectFile>();
                        
                        var pattern = string.Format(@"{0}\..*{1}", classNameBeingRenamed, testClassSuffix);//e.g. Class1.SecurityTests
                        targetProjects.ForEach(p => p.Project.Accept(new ProjectFileFinder(projectFilesWithMatchingName
                            , new Regex(pattern)
                            , new Regex(classNameBeingRenamed + testClassSuffix))));

                        foreach (var projectFile in projectFilesWithMatchingName)
                        {
                            if (targetProjects.Any(p => p.SubNamespaceFolder.FullPath == projectFile.Location.Directory.FullPath))
                            {
                                if (projectFile.Name.StartsWith(classNameBeingRenamed))//just to be sure
                                {
                                    var newTestClassName = newName + projectFile.Location.NameWithoutExtension.Substring(classNameBeingRenamed.Length);
                                    ResharperHelper.AppendLineToOutputWindow("Renaming {0} to {1}".FormatEx(projectFile.Name, newTestClassName));                                    
                                    EditorManager.GetInstance(solution).OpenProjectFile(projectFile, false);//need to ensure class within file is renamed tooo
                                    
                                    yield return new FileRename(psiModule.GetPsiServices(), projectFile, newTestClassName);
                                }
                            }  
                        }                   
                    }
                }                
            }                
        }

        private TestFileAnalysisSettings Settings
        {
            get { return TestCopSettingsManager.Instance.Settings; }
        }
    }
}
