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
    public class RenameTestFilesTooRefactoringTestProjectPerCodeProjectStrategyTests :
        RenameTestFilesTooRefactoringTestBase
    {
        protected override string RelativeTestDataPath
        {
            get { return @"MultipleTestProjectToSingleCodeProject\ClassToTestNavigation"; }
        }

        protected override string SolutionName
        {
            get { return @"TestApplication.sln"; }
        }

        protected override void ConfigureForTestCopStrategy(IContextBoundSettingsStore settingsStore)
        {
            MultipleTestProjectToSingleCodeProjectViaNamespace.ClassToTestFileNavigationTests.SetupTestCopSettings(settingsStore);
        }

        [Test]
        public void RenameClassRenamesTestFilesTooTest()
        {                        
            DoRenameTest(@"<TestApplication>\NG1\ClassWithBoth.cs", 1
                ,@"<TestApplication.IntegrationTests>\NG1\ClassWithBothTests.cs->NewClassTests"
                ,@"<TestApplication.Tests>\NG1\ClassWithBothTests.cs->NewClassTests" );      
        }
    }
}