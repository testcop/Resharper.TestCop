// --
// -- TestCop http://github.com/testcop
// -- License http://github.com/testcop/license
// -- Copyright 2016
// --
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper.Mapper
{
    public abstract class MappingBase : IProjectMappingHeper
    {
        public abstract IList<TestCopProjectItem> GetAssociatedProject(IProject currentProject, string className, string currentNameSpace, IList<Tuple<string, bool>> subDirectoryElements);

        public IList<TestCopProjectItem> GetAssociatedProjectFor(IProject currentProject, IProjectFile projectFile, string overrideClassName=null)
        {
            string currentNamespace = projectFile.CalculateExpectedNamespace(projectFile.GetPrimaryPsiFile().Language);

            var fileNameToProcess = projectFile.Location.NameWithoutExtension;
            fileNameToProcess = fileNameToProcess.RemoveTrailing(".partial");

            var directoryPath = TestCopProjectItem.ExtractFolders(projectFile).AsIList();

            return GetAssociatedProject(currentProject, string.IsNullOrEmpty(overrideClassName) ? fileNameToProcess : overrideClassName, currentNamespace, directoryPath);
        }
        
        public virtual bool IsTestProject(IProject project)
        {
            string currentProjectNamespace = project.GetDefaultNamespace();
            if (string.IsNullOrEmpty(currentProjectNamespace)) return false;

            return this.TestNameSpaceRegEx.IsMatch(currentProjectNamespace);
        }

        protected virtual Regex TestNameSpaceRegEx
        {
            get
            {
                string testNameSpacePattern = Settings.TestProjectToCodeProjectNameSpaceRegEx;
                Regex regEx = new Regex(testNameSpacePattern);
                return regEx;
            }
        }

        public void DumpDebug(ISolution solution)
        {
            Regex testNameSpaceRegEx = this.TestNameSpaceRegEx;

            foreach (IProject project in solution.GetAllCodeProjects())
            {
                string projectDefaultNameSpace = project.GetDefaultNamespace();
                bool matchesTestNameSpace = testNameSpaceRegEx.IsMatch(projectDefaultNameSpace ?? "");
                string matchResult = matchesTestNameSpace ? " matches " : " does not match ";

                ResharperHelper.AppendLineToOutputWindow(solution.Locks,
                    $"\tProject Namespace:{projectDefaultNameSpace}{matchResult}{testNameSpaceRegEx}");
            }
        }

        protected static TestFileAnalysisSettings Settings
        {
            get { return TestCopSettingsManager.Instance.Settings; }
        }

        public static bool RegexReplace(string regexPattern, string regexReplaceText, string inputString, out string resultString)
        {
            resultString = "";
            var regex = new Regex(regexPattern);
            var match = regex.Match(inputString);

            if (match.Success && match.Groups.Count > 1)
            {
                if (regexReplaceText.IsNullOrEmpty() || regexReplaceText == "*")
                {
                    for (int i = 1; i < match.Groups.Count; i++) resultString += match.Groups[i].Value;
                    return true;
                }

                resultString = regex.Replace(inputString, regexReplaceText);
                return true;
            }
            return false;
        }


        protected static IEnumerable<TestCopProjectItem.FilePatternMatcher> AssociatedFileNames(TestFileAnalysisSettings settings, string className)
        {        
            string classNameUnderTest = className;

            foreach (var suffix in settings.TestClassSuffixes())
            {
                if (className.EndsWith(suffix))
                {
                    classNameUnderTest = className.Split(new[] { '.' }, 2)[0].RemoveTrailing(suffix);
                    break;
                }
            }

            if (className != classNameUnderTest)
            {
                yield return new TestCopProjectItem.FilePatternMatcher(new Regex(classNameUnderTest), "");
            }
            else
            {
                foreach (var suffix in settings.TestClassSuffixes())
                {
                    yield return new TestCopProjectItem.FilePatternMatcher(new Regex(string.Format(@"{0}{1}", classNameUnderTest, suffix)), suffix);//e.g. Class1Tests
                    yield return new TestCopProjectItem.FilePatternMatcher(new Regex(string.Format(@"{0}\..*{1}", classNameUnderTest, suffix)), suffix);  //e.g. Class1.SecurityTests                  
                }
            }
        }
    }
}