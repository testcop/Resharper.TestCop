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
                        var projectTestFilesWithMatchingName = new List<IProjectFile>();
                        
                        var pattern = string.Format(@"{0}\..*{1}", classNameBeingRenamed, testClassSuffix);//e.g. Class1.SecurityTests
                        targetProjects.ForEach(p => p.Project.Accept(new ProjectFileFinder(projectTestFilesWithMatchingName
                            , new Regex(pattern)
                            , new Regex(classNameBeingRenamed + testClassSuffix))));

                        foreach (var projectTestFile in projectTestFilesWithMatchingName)
                        {
                            //for the test files that match we reverse lookup to see if they map to code file location
                            var codeProject=ProjectMappingHelper.GetProjectMappingHeper()
                                .GetAssociatedProject(projectTestFile.GetProject(), projectTestFile.CalculateExpectedNamespace(projectTestFile.GetPrimaryPsiFile().Language));

                            if (codeProject.Any(codefile => codefile.SubNamespaceFolder == declaredElement.GetSourceFiles().First().GetLocation().Directory))
                            {
                                if (projectTestFile.Name.StartsWith(classNameBeingRenamed)) //just to be sure
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
                            else
                            {
                                 codeProject.ForEach(p=>
                                ResharperHelper.AppendLineToOutputWindow(
                                "Skipped {0} as it's associated code folder {1}<>{2}".FormatEx(
                                    projectTestFile.Location.FullPath,
                                    p.SubNamespaceFolder.FullPath
                                    ,declaredElement.GetSourceFiles().First().GetLocation().Directory.FullPath)));
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
