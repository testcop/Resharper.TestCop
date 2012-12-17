using System.Collections.Generic;
using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.ReSharper.Settings;

namespace TestCop
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
        
        [SettingsEntry("TestClass,TestMethod,TestFixture,Test", "Testing Attributes to detect")]
        public string TestingAttributeText { get; set; }

        [SettingsEntry("Given,When", "Context prefixes for class names")]
        public string BddPrefix { get; set; }

        [SettingsEntry(false, "Do we look for any reference or just in files with similar names")]
        public bool FindAnyUsageInTestAssembly { get; set; }

        [SettingsEntry(true, "Check the namespace of the test matching the class under test")]
        public bool CheckTestNamespaces { get; set; }

        [SettingsEntry("Tests", "Suffix to always be applied to Test classes")]
        public string TestClassSuffix { get; set; }

        /*
        [SettingsEntry(true, "Prompt to create missing class file when switching between code & test")]
        public bool OfferToCreateAssociatedClass { get; set; }
        */
        
        
        
    }
}

