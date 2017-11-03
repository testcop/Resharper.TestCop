using System;
using System.Collections.Generic;
using NUnit.Framework;
using TestCop.Plugin.Helper.Mapper;

namespace TestCop.Plugin.Tests.Helper.Mapper
{
    [TestFixture]
    public class CodeProjectMapsToSingleTestProjectHeperTests
    {
        [TestFixture]
        public class AddMissingDirectoryElementsInNamespaceTests
        {
            [Test]
            public void DoesNotInsertExtraOnMatchTest()
            {
                // <MyCorp.App.DAL>.ClassA --> <MyCorp.App.Tests>.DAL.Entity1.Repository 
                var subNameSpaceOfTest = "DAL.Entity1.Repository";
                var subDirectoryElements = new List<Tuple<string, bool>>();

                subDirectoryElements.Add(new Tuple<string, bool>("DAL", true));
                subDirectoryElements.Add(new Tuple<string, bool>("Entity1", true));
                subDirectoryElements.Add(new Tuple<string, bool>("Repository", true));

                var expectedResult = new List<Tuple<string, bool>>(subDirectoryElements);
                var massagedPath =
                    CodeProjectMapsToSingleTestProjectHeper.AddMissingDirectoryElementsInNamespace(subDirectoryElements,
                        subNameSpaceOfTest);

                CollectionAssert.AreEqual(expectedResult, massagedPath);
            }

            [Test]
            public void DoesNotInsertExtraOnMatchMixedCaseTest()
            {
                // <MyCorp.App.DAL>.ClassA --> <MyCorp.App.Tests>.DAL.Entity1.Repository 
                var subNameSpaceOfTest = "DAL.Entity1.Repository";
                var subDirectoryElements = new List<Tuple<string, bool>>();

                subDirectoryElements.Add(new Tuple<string, bool>("Dal", true));
                subDirectoryElements.Add(new Tuple<string, bool>("ENTITY1", true));
                subDirectoryElements.Add(new Tuple<string, bool>("Repository", true));

                var expectedResult = new List<Tuple<string, bool>>(subDirectoryElements);
                var massagedPath =
                    CodeProjectMapsToSingleTestProjectHeper.AddMissingDirectoryElementsInNamespace(subDirectoryElements,
                        subNameSpaceOfTest);

                CollectionAssert.AreEqual(expectedResult, massagedPath);
            }

            [Test]
            public void DoesNotInsertExtraWithNonNamespaceFoldersTest()
            {
                // <MyCorp.App.DAL>.ClassA --> <MyCorp.App.Tests>.DAL.Entity1.Repository 
                var subNameSpaceOfTest = "DAL.Entity1.Repository";
                var subDirectoryElements = new List<Tuple<string, bool>>();

                subDirectoryElements.Add(new Tuple<string, bool>("Dal", true));
                subDirectoryElements.Add(new Tuple<string, bool>("ns-false", false));
                subDirectoryElements.Add(new Tuple<string, bool>("ENTITY1", true));
                subDirectoryElements.Add(new Tuple<string, bool>("ns-false", false));
                subDirectoryElements.Add(new Tuple<string, bool>("Repository", true));

                var expectedResult = new List<Tuple<string, bool>>(subDirectoryElements);
                var massagedPath =
                    CodeProjectMapsToSingleTestProjectHeper.AddMissingDirectoryElementsInNamespace(subDirectoryElements,
                        subNameSpaceOfTest);

                CollectionAssert.AreEqual(expectedResult, massagedPath);
            }

            [Test]
            public void InsertMissingFolderFromNamespaceTest()
            {
                // <MyCorp.App.DAL>.ClassA --> <MyCorp.App.Tests>.DAL.Entity1.Repository 
                var subNameSpaceOfTest = "DAL.Entity1.Repository";
                var subDirectoryElements = new List<Tuple<string, bool>>();

                subDirectoryElements.Add(new Tuple<string, bool>("ns-false", false));
                subDirectoryElements.Add(new Tuple<string, bool>("ENTITY1", true));
                subDirectoryElements.Add(new Tuple<string, bool>("ns-false", false));
                subDirectoryElements.Add(new Tuple<string, bool>("Repository", true));

                var expectedResult = new List<Tuple<string, bool>>(subDirectoryElements);

                var massagedPath =
                    CodeProjectMapsToSingleTestProjectHeper.AddMissingDirectoryElementsInNamespace(subDirectoryElements,
                        subNameSpaceOfTest);

                expectedResult.Insert(0, new Tuple<string, bool>("DAL", true));

                CollectionAssert.AreEqual(expectedResult, massagedPath);
            }

            [Test]
            public void AcceptsInvalidInputsTest()
            {
                var emptyElements = new List<Tuple<string, bool>>();
                var subDirectoryElements = new List<Tuple<string, bool>>();
                subDirectoryElements.Add(new Tuple<string, bool>("ns-false", false));

                CodeProjectMapsToSingleTestProjectHeper.AddMissingDirectoryElementsInNamespace(emptyElements, "");
                CodeProjectMapsToSingleTestProjectHeper.AddMissingDirectoryElementsInNamespace(subDirectoryElements, "");

                CodeProjectMapsToSingleTestProjectHeper.AddMissingDirectoryElementsInNamespace(emptyElements,"DAL.NS1.Repository");
                CodeProjectMapsToSingleTestProjectHeper.AddMissingDirectoryElementsInNamespace(subDirectoryElements,"DAL.NS1.Repository");
            }
        }
    }
}
