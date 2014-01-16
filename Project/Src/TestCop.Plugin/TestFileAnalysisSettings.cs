﻿// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.DataFlow;
using JetBrains.ReSharper.Settings;

namespace TestCop.Plugin
{    
    [SettingsKey(typeof (CodeInspectionSettings), "Testing Attributes")]
    public class TestFileAnalysisSettings
    {     
        public IList<string> TestingAttributes 
        { 
            get
            {
                List<string> list = (TestingAttributeText ?? "").Split(',').ToList();
                list.RemoveAll(string.IsNullOrEmpty);
                return list;
            }
        }

        public IList<string> BddPrefixes
        {
            get
            {
                List<string> list = (BddPrefix ?? "").Split(',').ToList();
                list.RemoveAll(string.IsNullOrEmpty);
                return list;
            }
        }
        
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

        [SettingsEntry(@"^(.*?)\.?Tests$", "Regex to identify tests project by their namespace")]
        public string TestProjectToCodeProjectNameSpaceRegEx { get; set; }

        [SettingsEntry(@"", "RegEx replacement text")]
        public string TestProjectToCodeProjectNameSpaceRegExReplace { get; set; }

        [SettingsEntry(@"Global::Ctrl+G, Ctrl+T", "Keyboard shortcut for switching between code and unit test files")]
        public string ShortcutToSwitchBetweenFiles { get; set; }

        [SettingsEntry(@"Class", "Name of Template to use when creating a code class")]
        public string CodeFileTemplateName { get; set; }

        [SettingsEntry(@"Class", "Name of Template to use when creating a unittest class")]
        public string UnitTestFileTemplateName { get; set; }


    }

    [ShellComponent]
    public class TestCopSettingsManager
    {
        private readonly Lifetime _lifetime;
        private readonly ISettingsStore _settingsStore;

        public TestCopSettingsManager(Lifetime lifetime, ISettingsStore settingsStore)
        {
            _lifetime = lifetime;
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

                IContextBoundSettingsStore context = _settingsStore.BindToContextLive(_lifetime, ContextRange.ApplicationWide);
                
                var testFileAnalysisSettings =
                    context.GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);
                return testFileAnalysisSettings;
            }
        } 
    }
}

