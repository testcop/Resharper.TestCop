// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2014
// --

using System.IO;
using JetBrains.ActionManagement;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Daemon;
using NUnit.Framework;
using TestCop.Plugin.Highlighting;

namespace TestCop.Plugin.Tests.SingleTestProjectToMultipleCodeProject
{
    [TestFixture]
    public class TestToClassFileNavigationTests : CSharpHighlightingWithinSolutionTestBase
    {
        protected override bool HighlightingPredicate(IHighlighting highlighting, IContextBoundSettingsStore settingsstore)
        {
            return highlighting is TestFileNameSpaceWarning;
        }

        protected override string RelativeTestDataPath
        {
            get { return @"SingleTestProjectForManyCodeProject\TestToClassNavigation"; }
        }

        protected override IActionHandler GetShortcutAction(TextWriter textwriter)
        {
            IActionHandler jumpToTestFileAction = new JumpToTestFileAction(CreateJetPopMenuShowToWriterAction(textwriter));
            return jumpToTestFileAction;
        }
        protected override string SolutionName
        {
            get { return @"TestApplication3.sln"; }
        }

        [Test]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\API\ClassATests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\API\NS1\ClassATests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\API\NS1\NS2\ClassCTests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\API\NS1\NS2\ClassATests.cs")]        
        [TestCase(@"<MyCorp.TestApplication3.Tests>\ClassNotValidAtRootTests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\DAL\ClassATests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\DAL\NS1\ClassATests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\DAL\NS1\NS2\ClassATests.cs")]
        [TestCase(@"<MyCorp.TestApplication3.Tests>\DAL\NS1\NS2\DALClassDWithNoCodeTests.cs")]        
        public void Test(string testName)
        {   
             // http://myregexp.com/
            string regexTestToAssembly = @"^(.*?)\.?Tests(\..*?)(\..*)*$";
            string regexTestToAssemblyProjectReplace = "$1$2";
            string regexTestToAssemblyProjectSubNamespaceReplace = "$3";

            string regexCodeToTestAssembly = @"^(.*?\..*?)(\..*?)$";
            string regexCodeToTestReplace = "$2";

            ExecuteWithinSettingsTransaction((settingsStore =>
            {
                RunGuarded(
                    () =>
                    {
                        settingsStore.SetValue<TestFileAnalysisSettings, bool>(
                            s => s.ConfiguredForSingleTestProject, true);
                    }

                    );
                DoTestFiles(testName);
            }));
        }
    }
}
