// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2015
// --
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains;
using JetBrains.IDE;
using JetBrains.Metadata.Reader.API;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Refactorings.Specific.Rename;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.Refactorings.Rename;
using JetBrains.Util;
using TestCop.Plugin.Extensions;
using TestCop.Plugin.Helper;
using TestCop.Plugin.Helper.Mapper;

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
                    var projectFile = typeElement.GetSourceFiles().First().ToProjectFile();
                    if (projectFile.Name != classNameBeingRenamed + projectFile.Location.ExtensionWithDot)
                    {
                        ResharperHelper.AppendLineToOutputWindow("# skipped as file name doesnt match class being renamed");
                        yield break;
                    }

                    //get associated projects..
                    var targetProjects = project.GetAssociatedProjects(projectFile);
                    if (targetProjects.IsEmpty())
                    {
                        yield break;
                    }
                    
                    //now look for expected file names that are also in correct locations
                    foreach (var targetProject in targetProjects)
                    {
                        var projectTestFilesWithMatchingName = new List<IProjectFile>();
                        targetProject.Project.Accept(new ProjectFileFinder(projectTestFilesWithMatchingName, targetProject.FilePattern));
                        
                        foreach (var projectTestFile in projectTestFilesWithMatchingName)
                        {
                            string expectedNameSpace =
                                projectTestFile.CalculateExpectedNamespace(projectTestFile.GetPrimaryPsiFile().Language);

                            if (expectedNameSpace==targetProject.FullNamespace())
                            {                                                                                          
                                    var newTestClassName = newName +
                                                           projectTestFile.Location.NameWithoutExtension.Substring(
                                                               classNameBeingRenamed.Length);
                                    ResharperHelper.AppendLineToOutputWindow(
                                        "Renaming {0} to {1}".FormatEx(projectTestFile.Location.FullPath, newTestClassName));
                                    if (projectTestFile.Location.NameWithoutExtension == newTestClassName)
                                    {
                                        ResharperHelper.AppendLineToOutputWindow("# skip as same name");
                                        continue;
                                    }

                                    EditorManager.GetInstance(solution).OpenProjectFile(projectTestFile, false);
                                        //need to ensure class within file is renamed tooo                                    
                                    yield return
                                        new FileRename(psiModule.GetPsiServices(), projectTestFile, newTestClassName);                                
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
