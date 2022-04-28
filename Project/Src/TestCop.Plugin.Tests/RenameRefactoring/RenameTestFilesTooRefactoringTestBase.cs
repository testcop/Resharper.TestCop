﻿// // --
// // -- TestCop http://github.com/testcop
// // -- License http://github.com/testcop/license
// // -- Copyright 2015
// // --

using System;
using System.Diagnostics;
using System.Linq;

using JetBrains.Application.Components;
using JetBrains.Application.Settings;
using JetBrains.Application.Threading;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.Lifetimes;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Projects;
using JetBrains.Util;

using NUnit.Framework;

using TestCop.Plugin.Helper;

namespace TestCop.Plugin.Tests.RenameRefactoring
{
    using System.Collections.Generic;

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
            this.RunGuarded((Action)(() => Lifetime.Using((Action<Lifetime>)(lifetime =>
            {                
                ConfigureForTestCopStrategy(settingsStore);

                settingsStore.SetValue<TestFileAnalysisSettings, bool>(s => s.SupportRenameRefactor, true);
                
                ISolution solution;
                using (Locks.UsingWriteLock())
                {
                    VirtualFileSystemPath solutionPath = this.VirtualTestDataPath.Combine(this.SolutionName);
                    solution = (ISolution)this.SolutionManager.OpenExistingSolution(solutionPath);
                }

                lifetime.OnTermination(() => SolutionManager.CloseSolution(solution));

                var findFirstTypeInFile = FindTypeInFile(solution, testFile, typeSequenceInFile);

                var fileRenames = new RenameTestFilesTooRefactoring().GetFileRenames(findFirstTypeInFile, "NewClass");

                var filesToRename = fileRenames.Select(f => f.ProjectItem.GetPresentableProjectPath()+ "->" + f.NewName).ToList();

                Assert.AreEqual(expectedRenamedTests.Length, filesToRename.Count);

                foreach (string expectation in expectedRenamedTests)
                {
                    CollectionAssert.Contains(filesToRename, expectation);
                }
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
                IEnumerable<IProjectFile> projectFiles = solution.GetAllProjects().SelectMany(p => p.GetAllProjectFiles());

                foreach (IProjectFile file in projectFiles)
                {
                    Debug.WriteLine(file.GetPresentableProjectPath());
                }
                throw new Exception("Whilst configuring test I didn't find project item: "+testFile);
            }

            DocumentManager documentManager = solution.GetComponent<DocumentManager>();
            var document = documentManager.GetOrCreateDocument(projectFile);
            var findFirstTypeInFile = ResharperHelper.FindDeclaredElementInFile(solution, document, typeSequenceInFile);
                return findFirstTypeInFile;                                                      
        }
    }
}