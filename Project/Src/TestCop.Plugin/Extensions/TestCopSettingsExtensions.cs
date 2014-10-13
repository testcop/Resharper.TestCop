
using System.Collections.Generic;
using System.Linq;


namespace TestCop.Plugin.Extensions
{
    public static class TestCopSettingsExtensions
    {
        public static IEnumerable<string> GetAppropriateTestClassSuffixes(this TestFileAnalysisSettings settings,
            string baseFileName)
        {
            var testClassSuffixes = settings.TestClassSuffixes();

            if (baseFileName.EndsWith(testClassSuffixes))
            {
                return new[] {testClassSuffixes.First(baseFileName.EndsWith)};
            }
            return testClassSuffixes;
        }

        public static IList<string> TestingAttributes(this TestFileAnalysisSettings settings)
        {

            List<string> list = (settings.TestingAttributeText ?? "").Split(',').ToList();
            list.RemoveAll(string.IsNullOrEmpty);
            return list;
        }

        public static IList<string> TestClassSuffixes(this TestFileAnalysisSettings settings)
        {

            List<string> list = (settings.TestClassSuffix ?? "").Split(',').ToList();
            list.RemoveAll(string.IsNullOrEmpty);
            list.Sort((a, b) => b.Length - a.Length);
            return list;
        }

        public static IList<string> BddPrefixes(this TestFileAnalysisSettings settings)
        {

            List<string> list = (settings.BddPrefix ?? "").Split(',').ToList();
            list.RemoveAll(string.IsNullOrEmpty);
            return list;
        }
    }
}
