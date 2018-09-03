// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2016
// --
using System.Collections.Generic;
using JetBrains;
using JetBrains.IDE;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Refactorings.Specific.Rename;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
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
            

           if (typeElement != null)//only support renaming of 'types' 
            {
                var classNameBeingRenamed = declaredElement.ShortName;
                var project = psiModule.Project;
                var solution = project.GetSolution();

                if (!project.IsTestProject())
                {
                    var projectFiles = declaredElement.GetSourceFiles().ToList(x => x.ToProjectFile());
                    //get associated projects..

                    IList<TestCopProjectItem> targetProjects = new List<TestCopProjectItem>();
                    foreach (var projectFile in projectFiles)
                    {
                        targetProjects.AddRange(project.GetAssociatedProjects(projectFile, classNameBeingRenamed));
                    }
                    
                    //var targetProjects = project.GetAssociatedProjects(projectFiles);
                    if (targetProjects.IsNullOrEmpty())
                    {
                        yield break;
                    }
                    
                    //now look for expected file names that are also in correct locations
                    foreach (var targetProject in targetProjects)
                    {
                        var projectTestFilesWithMatchingName = new List<ProjectFileFinder.Match>();
                        targetProject.Project.Accept(new ProjectFileFinder(projectTestFilesWithMatchingName, targetProject.FilePattern));
                        
                        foreach (var projectFileMatch in projectTestFilesWithMatchingName)
                        {
                            string expectedNameSpace =
                                projectFileMatch.ProjectFile.CalculateExpectedNamespace(projectFileMatch.ProjectFile.GetPrimaryPsiFile().Language);

                            if (expectedNameSpace==targetProject.FullNamespace())
                            {
                                string currentName=projectFileMatch.ProjectFile.Location.NameWithoutExtension;
                                var newTestClassName = newName + currentName.Substring(classNameBeingRenamed.Length);
                                   
                                ResharperHelper.AppendLineToOutputWindow(project.Locks,
                                        "Renaming {0} to {1}".FormatEx(projectFileMatch.ProjectFile.Location.FullPath, newTestClassName));
                                    if (projectFileMatch.ProjectFile.Location.NameWithoutExtension == newTestClassName)
                                    {
                                        ResharperHelper.AppendLineToOutputWindow(project.Locks, "# skip as same name");
                                        continue;
                                    }

                                    EditorManager.GetInstance(solution).OpenProjectFile(projectFileMatch.ProjectFile, new OpenFileOptions(false));
                                        //need to ensure class within file is renamed tooo                                    
                                    yield return
                                        new FileRename(psiModule.GetPsiServices(), projectFileMatch.ProjectFile, newTestClassName);                                
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
