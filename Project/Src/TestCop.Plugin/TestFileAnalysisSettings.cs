using System.Collections.Generic;
using System.Linq;
using JetBrains.Application;
using JetBrains.Application.Settings;
using JetBrains.Application.Settings.Store.Implementation;
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

        [SettingsEntry(".Tests", "Suffix to always be applied to the namespace of Tests - include . if needed")]
        public string TestNameSpaceSuffix { get; set; }  
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

                IContextBoundSettingsStoreLive contextLive = _settingsStore.BindToContextLive(_lifetime, ContextRange.ApplicationWide);
                
                var testFileAnalysisSettings =
                    contextLive.GetKey<TestFileAnalysisSettings>(SettingsOptimization.OptimizeDefault);
                return testFileAnalysisSettings;
            }
        } 
    }
}

