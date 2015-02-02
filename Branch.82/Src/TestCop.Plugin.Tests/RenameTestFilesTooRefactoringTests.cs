// // --
// // -- TestCop http://testcop.codeplex.com
// // -- License http://testcop.codeplex.com/license
// // -- Copyright 2014
// // --

using System;
using System.Linq;
using JetBrains.Application.Components;
using JetBrains.Application.Progress;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.DocumentManagers;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Refactorings.Rename;
using JetBrains.TestFramework.ProjectModel;
using JetBrains.TestShell.Infra;
using JetBrains.Util;
using NUnit.Framework;
using TestCop.Plugin.Helper;

namespace TestCop.Plugin.Tests
{
    [TestFixture]
    public class RenameTestFilesTooRefactoringTests : BaseTest
    {        
        protected override string RelativeTestDataPath
        {
            get { return @"MultipleTestProjectToSingleCodeProject\ClassToTestNavigation"; }
        }
  
        private TestSolutionManager SolutionManager
        {
            get
            {
                return ComponentContainerEx.GetComponent<TestSolutionManager>(this.ShellInstance);
            }
        }

        protected string SolutionName
        {
            get { return @"TestApplication.sln"; }
        }

        [Test]
        public void RenameClassRenamesTestFilesTooTest()
        {
            const string altRegEx = "^(.*?)\\.?(Integration)*Tests$";
            string testFile = @"<TestApplication>\NG1\ClassWithBoth.cs";

            ExecuteWithinSettingsTransaction((settingsStore =>
            {
            this.RunGuarded((Action)(() => Lifetimes.Using((Action<Lifetime>)(lifetime =>
            {
                settingsStore.SetValue<TestFileAnalysisSettings, bool>(
                            s => s.SupportRenameRefactor, true);

                settingsStore.SetValue<TestFileAnalysisSettings, string>(
                    s => s.TestClassSuffix, "Tests,IntegrationTests");

                settingsStore.SetValue<TestFileAnalysisSettings, string>(
                    s => s.TestProjectToCodeProjectNameSpaceRegEx, altRegEx);
                settingsStore.SetValue<TestFileAnalysisSettings, string>(
                    s => s.TestProjectToCodeProjectNameSpaceRegExReplace, "$1");
                
                ISolution solution;
                using (this.Locks.UsingWriteLock())
                {
                    var solutionFolder = this.CopyTestDataDirectoryToTemp(lifetime,@"..\..\"+RelativeTestDataPath);
                    solution = (ISolution)this.SolutionManager.OpenExistingSolution(FileSystemPath.Parse(solutionFolder).Combine(SolutionName));
                }
                lifetime.AddAction(() => SolutionManager.CloseSolution(solution));

                var findFirstTypeInFile = FindFirstTypeInFile(solution, testFile);

                var fileRenames = new RenameTestFilesTooRefactoring().GetFileRenames(findFirstTypeInFile, "NewClass");

                var filesToRename = fileRenames.Select(f => f.ProjectItem.GetProject().Name+"."+f.ProjectItem.Name+ "->" + f.NewName).ToList();
                
                Assert.AreEqual(2, filesToRename.Count);
                Assert.Contains("TestApplication.IntegrationTests.ClassWithBothTests.cs->NewClassTests", filesToRename);
                Assert.Contains("TestApplication.Tests.ClassWithBothTests.cs->NewClassTests", filesToRename);

                /*
                  using (IProjectModelTransactionCookie resource_0 = SolutionEx.CreateTransactionCookie(solution, DefaultAction.Commit, "Test", (IProgressIndicator)NullProgressIndicator.Instance))
                       resource_0.Rename(projectFile, "NewClassName.cs");
                    */
            }))));
        }));
    }

        private static IDeclaredElement FindFirstTypeInFile(ISolution solution, string testFile)
        {
            var projectFile =
                solution.GetAllProjects()
                    .SelectMany(p => p.GetAllProjectFiles()).Single(p => p.GetPresentableProjectPath() == testFile);

            var document = DocumentManager.GetInstance(solution).GetOrCreateDocument(projectFile);
            var findFirstTypeInFile = ResharperHelper.FindFirstDeclaredElementInFile(solution, document);
            return findFirstTypeInFile;
        }
    }
}