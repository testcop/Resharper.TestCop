// // --
// // -- TestCop http://testcop.codeplex.com
// // -- License http://testcop.codeplex.com/license
// // -- Copyright 2016
// // --
using JetBrains.Application.Settings;
using NUnit.Framework;

namespace TestCop.Plugin.Tests.RenameRefactoring
{
    [TestFixture]
    public class RenameTestFilesTooRefactoringSingleTestProjectPerSolutionTests : RenameTestFilesTooRefactoringTestBase
    {        
        protected override string RelativeTestDataPath
        {
            get { return @"SingleTestProjectForManyCodeProject\ClassToTestNavigation"; }
        }
        
        protected override string SolutionName
        {
            get { return @"TestApplication3.sln"; }
        }

        protected override void ConfigureForTestCopStrategy(IContextBoundSettingsStore settingsStore)
        {
            SingleTestProjectToMultipleCodeProject.ClassToTestFileNavigationTests.SetupTestCopSettings(settingsStore);
        }

        [Test]
        public void RenameClassRenamesTestFilesTooTest()
        {            
            DoRenameTest(
                  @"<MyCorp.TestApplication3.DAL>\NS1\NS2\ClassA.cs", 1
                , @"<MyCorp.TestApplication3.Tests>\DAL\NS1\NS2\ClassATests.cs->NewClassTests");
        }

        [Test]
        public void RenameClassHandlesFirstClassWithilesTooTest()
        {
            DoRenameTest(
                  @"<MyCorp.TestApplication3.DAL>\ClassX.cs", 1
                , @"<MyCorp.TestApplication3.Tests>\DAL\ClassXTests.cs->NewClassTests");

        }    

        [Test]
        public void RenameClassCanProcessSecondClassWithinFileTest()
        {
            DoRenameTest(@"<MyCorp.TestApplication3.DAL>\ClassX.cs",                
                2 /*select ClassXX within the file (not ClassX) */  
                ,@"<MyCorp.TestApplication3.Tests>\DAL\ClassXXTests.cs->NewClassTests");
        }    
    }
}