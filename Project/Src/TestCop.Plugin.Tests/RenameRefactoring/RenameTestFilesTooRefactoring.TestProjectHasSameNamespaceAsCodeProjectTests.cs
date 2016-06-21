// // --
// // -- TestCop http://testcop.codeplex.com
// // -- License http://testcop.codeplex.com/license
// // -- Copyright 2015
// // --

using JetBrains.Application.Settings;
using NUnit.Framework;

namespace TestCop.Plugin.Tests.RenameRefactoring
{
    [TestFixture]
    public class RenameTestFilesTooRefactoringTestProjectHasSameNamespaceAsCodeProjectTests :
        RenameTestFilesTooRefactoringTestBase
    {
        protected override string RelativeTestDataPath
        {
            get { return @"MultipleTestProjectToSingleCodeProjectViaName\ClassToTestNavigation"; }
        }

        protected override string SolutionName
        {
            get { return @"MyCorp.TestApplication4.sln"; }
        }

        protected override void ConfigureForTestCopStrategy(IContextBoundSettingsStore settingsStore)
        {
            MultipleTestProjectToSingleCodeProjectViaProjectName.ClassToTestFileNavigationTests.SetupTestCopSettings(settingsStore);
        }

        [Test]
        public void RenameClassRenamesTestFilesTooTest()
        {
            DoRenameTest(@"<API>\NS1\ClassA.cs", 1
                , @"<APITests>\NS1\ClassATests.cs->NewClassTests");      
        }
    }
}