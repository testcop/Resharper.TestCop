// // --
// // -- TestCop http://testcop.codeplex.com
// // -- License http://testcop.codeplex.com/license
// // -- Copyright 2015
// // --

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Components;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Projects;
using JetBrains.Util;
using NUnit.Framework;
using TestCop.Plugin.Helper;

namespace TestCop.Plugin.Tests.RenameRefactoring
{
    [TestFixture]
    public abstract class RenameTestFilesTooRefactoringTestBase : BaseTest
    {        
        private TestSolutionManager SolutionManager
        {
            get
            {
                return ComponentContainerEx.GetComponent<TestSolutionManager>(ShellInstance);
            }
        }

        protected abstract string SolutionName { get; }

        protected abstract void ConfigureForTestCopStrategy(IContextBoundSettingsStore settingsStore);

        public void DoRenameTest(string testFile, int typeSequenceInFile=1, params string[] expectedRenamedTests)
        {            
            ExecuteWithinSettingsTransaction((settingsStore =>
            {
            this.RunGuarded((Action)(() => Lifetimes.Using((Action<Lifetime>)(lifetime =>
            {                
                ConfigureForTestCopStrategy(settingsStore);

                settingsStore.SetValue<TestFileAnalysisSettings, bool>(s => s.SupportRenameRefactor, true);
                
                ISolution solution;
                using (Locks.UsingWriteLock())
                {
                    var solutionFolder = this.CopyTestDataDirectoryToTemp(lifetime,@"..\..\"+RelativeTestDataPath);
                    solution = (ISolution)this.SolutionManager.OpenExistingSolution(FileSystemPath.Parse(solutionFolder).Combine(SolutionName));
                }
                lifetime.AddAction(() => SolutionManager.CloseSolution(solution));

                var findFirstTypeInFile = FindTypeInFile(solution, testFile, typeSequenceInFile);

                var fileRenames = new RenameTestFilesTooRefactoring().GetFileRenames(findFirstTypeInFile, "NewClass");

                var filesToRename = fileRenames.Select(f => f.ProjectItem.GetPresentableProjectPath()+ "->" + f.NewName).ToList();

                Assert.AreEqual(expectedRenamedTests.Length, filesToRename.Count);

                expectedRenamedTests.ForEach(expectation=>CollectionAssert.Contains(filesToRename, expectation));                                    
            }))));
        }));
    }
       
        private static IDeclaredElement FindTypeInFile(ISolution solution, string testFile, int typeSequenceInFile)
        {
                var projectFile =
                    solution.GetAllProjects()
                        .SelectMany(p => p.GetAllProjectFiles()).SingleOrDefault(p => p.GetPresentableProjectPath() == testFile);

            if (projectFile == null)
            {
                solution.GetAllProjects().SelectMany(p => p.GetAllProjectFiles()).ForEach(p=>Debug.WriteLine(p.GetPresentableProjectPath()));
                throw new Exception("Whilst configuring test I didn't find project item: "+testFile);
            }

            var document = DocumentManager.GetInstance(solution).GetOrCreateDocument(projectFile);
            var findFirstTypeInFile = ResharperHelper.FindDeclaredElementInFile(solution, document, typeSequenceInFile);
                return findFirstTypeInFile;                                                      
        }
    }
}