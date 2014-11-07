﻿// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Settings;

namespace TestCop.Plugin
{
    public enum TestProjectStrategy
    {
        TestProjectPerCodeProject=1,
        SingleTestProjectPerSolution=2,
        TestProjectHasSameNamespaceAsCodeProject=3
    }

    [SettingsKey(typeof (CodeInspectionSettings), "Testing Attributes")]
    public class TestFileAnalysisSettings
    {        
        [SettingsEntry("TestClass,TestMethod,TestFixture,Test,Fact", "Testing Attributes to detect")]
        public string TestingAttributeText { get; set; }

        [SettingsEntry("Given,When", "Context prefixes for class names")]
        public string BddPrefix { get; set; }

        [SettingsEntry(false, "Do we look for any reference or just in files with similar names")]
        public bool FindAnyUsageInTestAssembly { get; set; }

        [SettingsEntry(true, "Check the namespace of the test matching the class under test")]
        public bool CheckTestNamespaces { get; set; }

        [SettingsEntry("Tests", "Suffix to always be applied to Test classes")]
        public string TestClassSuffix { get; set; }

        [SettingsEntry(@"Global::Ctrl+G, Ctrl+T", "Keyboard shortcut for switching between code and unit test files")]
        public string ShortcutToSwitchBetweenFiles { get; set; }

        [SettingsEntry(@"Class", "Name of Template to use when creating a code class")]
        public string CodeFileTemplateName { get; set; }

        [SettingsEntry(@"Class", "Name of Template to use when creating a unittest class")]
        public string UnitTestFileTemplateName { get; set; }   
        
        [SettingsEntry(@"true", "Should the TestCop output panel be opened on startup")]
        public bool OutputPanelOpenOnKeyboardMapping { get; set; }

        [SettingsEntry(@"^(.*?)\.?Tests$", "Regex to identify tests project by their namespace")]
        public string TestProjectToCodeProjectNameSpaceRegEx { get; set; }

        [SettingsEntry(@"", "RegEx replacement text")]
        public string TestProjectToCodeProjectNameSpaceRegExReplace { get; set; }
  
        [SettingsEntry(@TestProjectStrategy.TestProjectPerCodeProject, "Which strategy should testcop use for mapping tests to code")]
        public TestProjectStrategy TestCopStrategy { get; set; }
        
        [SettingsEntry(@"^(.*?)\.?Tests(\..*?)(\..*)*$", "Regex for test namespace within single test assembly solutions")]
        public string SingleTestRegexTestToAssembly { get; set; }

        [SettingsEntry(@"$1$2", "Regex replace for test namespace within single test assembly solutions to identify namespace of code assembly")]
        public string SingleTestRegexTestToAssemblyProjectReplace { get; set; }

        [SettingsEntry(@"$3", "Regex replace for test namespace within single test assembly solutions to identify sub-namespace of code assembly")]
        public string SingleTestRegexTestToAssemblyProjectSubNamespaceReplace { get; set; }

        [SettingsEntry(@"^(.*?\..*?)(\..*?)$", "Regex for code namespace within single test assembly solutions")]
        public string SingleTestRegexCodeToTestAssembly { get; set; }

        [SettingsEntry(@"$2", "Regex replace for code namespace within single test assembly solutions to identify namespace of test assembly")]
        public string SingleTestRegexCodeToTestReplace { get; set; }

        [SettingsEntry(true, "Search project folders for files not part of the project")]
        public bool FindOrphanedProjectFiles { get; set; }

        [SettingsEntry("*.cs|*.aspx|*.jpg", "Pattern for orphaned files")]
        public string OrphanedFilesPatterns { get; set; }

        [SettingsEntry(false, "Rename test files when renaming code file")]
        public bool SupportRenameRefactorBeta { get; set; }
    }

    [ShellComponent]
    public class TestCopSettingsManager
    {
        private readonly ISettingsStore _settingsStore;

        public TestCopSettingsManager(ISettingsStore settingsStore)
        {
            _settingsStore = settingsStore;
        }

        public static TestCopSettingsManager Instance
        {
            get
            {
                return Shell.Instance.GetComponent<TestCopSettingsManager>();
            }
        }

        public TestFileAnalysisSettings Settings
        {
            get
            {

                IContextBoundSettingsStore context = _settingsStore.BindToContextTransient(ContextRange.ApplicationWide);
                
                var testFileAnalysisSettings =
                    context.GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);
                return testFileAnalysisSettings;
            }
        } 
    }
}

