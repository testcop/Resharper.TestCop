// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2016
// --

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Util;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Helper.Mapper
{
    public class CodeProjectMapsToSingleTestProjectHeper : MappingBase
    {
        protected override Regex TestingRegEx
        {
            get
            {
                var testNameSpacePattern = Settings.SingleTestRegexTestToAssembly;
                var regEx = new Regex(testNameSpacePattern);
                return regEx;
            }
        }

        public override IList<TestCopProjectItem> GetAssociatedProject(IProject currentProject, string className, string currentTypeNamespace, IList<Tuple<string, bool>> subDirectoryElements)
        {
            var settings = Settings;
            const string warningMessage = "Not Supported: More than one code project has a default namespace of ";

            var filePatterns = AssociatedFileNames(Settings, className);
          
            if (currentProject.IsTestProject())
            {                
                // <MyCorp.App.Tests>.API.ClassA --> <MyCorp.App.API>.ClassA
                string nameSpaceOfAssociateProject;
                RegexReplace(settings.SingleTestRegexTestToAssembly
                    , settings.SingleTestRegexTestToAssemblyProjectReplace, currentTypeNamespace,
                    out nameSpaceOfAssociateProject);

                string subNameSpace;
                RegexReplace(settings.SingleTestRegexTestToAssembly
                    , settings.SingleTestRegexTestToAssemblyProjectSubNamespaceReplace, currentTypeNamespace,
                     out subNameSpace);

                var matchedCodeProjects = currentProject.GetSolution().GetNonTestProjects().Where(
                    p => p.GetDefaultNamespace()  == nameSpaceOfAssociateProject).ToList();
                

                if (matchedCodeProjects.Count() > 1)
                {
                    ResharperHelper.AppendLineToOutputWindow(warningMessage + nameSpaceOfAssociateProject);
                }

                if (matchedCodeProjects.Count == 0)
                {
                    ResharperHelper.AppendLineToOutputWindow("Didn't find project with namespace of: " + nameSpaceOfAssociateProject + " to match " + currentTypeNamespace);                    
                }

                var subDirectoryElementsWithOutExtraFolderForNS = new List<Tuple<string, bool>>(subDirectoryElements);

                RemoveRootFoldersPresentInNameSpace(subDirectoryElementsWithOutExtraFolderForNS, subNameSpace);

                if (subDirectoryElementsWithOutExtraFolderForNS.Count > 0)
                {
                    subDirectoryElementsWithOutExtraFolderForNS.RemoveAt(0); 
                }

                return matchedCodeProjects.Select(p => new TestCopProjectItem(p, TestCopProjectItem.ProjectItemTypeEnum.Code, subNameSpace, subDirectoryElementsWithOutExtraFolderForNS, filePatterns)).ToList();
            }

            // <MyCorp.App.API>.ClassA --> <MyCorp.App.Tests>.API.ClassA 
            string subNameSpaceOfTest;
            RegexReplace(settings.SingleTestRegexCodeToTestAssembly
                , settings.SingleTestRegexCodeToTestReplace, currentTypeNamespace,
                out subNameSpaceOfTest);
            
            var matchedTestProjects = currentProject.GetSolution().GetTestProjects().ToList();
            if (matchedTestProjects.Count > 1)
            {
                ResharperHelper.AppendLineToOutputWindow("Not Supported: Expected only one test project for all code projects to use");                
            }

            var subDirectoryElementsWithExtraFolderForNS = AddMissingDirectoryElementsInNamespace(subDirectoryElements, subNameSpaceOfTest);
            
            return matchedTestProjects.Select(p => new TestCopProjectItem(p, TestCopProjectItem.ProjectItemTypeEnum.Tests, subNameSpaceOfTest, subDirectoryElementsWithExtraFolderForNS, filePatterns)).Take(1).ToList();                                        
        }

        public static List<Tuple<string, bool>> AddMissingDirectoryElementsInNamespace(IList<Tuple<string, bool>> subDirectoryElements, string subNameSpaceOfTest)
        {
            // We gain root folders from the namespace: <MyCorp.App.API>.ClassA --> <MyCorp.App.Tests>.API.ClassA 
            var subDirectoryElementsWithExtraFolderForNS = new List<Tuple<string, bool>>(subDirectoryElements);
            var elemSubNameSpaceOfTest = subNameSpaceOfTest.Split('.').Where(x => !string.IsNullOrEmpty(x)).ToList();

            //compare namespace path with directories - skipping the ns=false folders         
            for (int j = subDirectoryElementsWithExtraFolderForNS.Count - 1, i = elemSubNameSpaceOfTest.Count - 1;i >= 0;i--)
            {
                while (j >= 0 && subDirectoryElementsWithExtraFolderForNS[j].Item2 == false) j--; //skip non-namespace providers

                if (j < 0 || String.Compare(elemSubNameSpaceOfTest[i], subDirectoryElementsWithExtraFolderForNS[j].Item1, StringComparison.InvariantCultureIgnoreCase)!=0 )
                {
                    subDirectoryElementsWithExtraFolderForNS.Insert(Math.Max(0, j),new Tuple<string, bool>(elemSubNameSpaceOfTest[i], true));
                }
                j = j - 1;
            }
            return subDirectoryElementsWithExtraFolderForNS;
        }

        public static List<Tuple<string, bool>> RemoveRootFoldersPresentInNameSpace(IList<Tuple<string, bool>> subDirectoryElements, string subNameSpace)
        {
            ///TODO: HOW TO HANDLE THIS TYPE OF TEST SETUP...
            /// 
            // We remove root folders now within the namespace: // <MyCorp.App.Tests>.API.ClassA --> <MyCorp.App.API>.ClassA
            var subDirectoryElementsWithExtraFolderForNS = new List<Tuple<string, bool>>(subDirectoryElements);
/*
            var elemSubNameSpaceOfTest = subNameSpaceOfTest.Split('.').Where(x => !string.IsNullOrEmpty(x)).ToList();

            //compare namespace path with directories - skipping the ns=false folders         
            for (int j = 0, i = elemSubNameSpaceOfTest.Count - 1; i >= 0; i++)
            {

                while (subDirectoryElementsWithExtraFolderForNS.Count > 0 &&
                       subDirectoryElementsWithExtraFolderForNS[0].Item2 == false) //strip out non-namespace providers
                {
                    subDirectoryElementsWithExtraFolderForNS.RemoveAt(0);
                }

                if (subDirectoryElementsWithExtraFolderForNS[0] == elemSubNameSpaceOfTest[i])
                {
                    subDirectoryElementsWithExtraFolderForNS.RemoveAt(0);
                }

            }
            */
            return subDirectoryElementsWithExtraFolderForNS;
        }
    }
}