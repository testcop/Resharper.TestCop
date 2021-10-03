using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;

namespace TestCop.Plugin.Tests
{
    [TestFixture]
    public class CheckAssemblyVersionTest
    {
        [Test]
        public void AssertVersionMatchesNugetReleaseNote()
        {
            // the assembly version should match the latest entry within the TestCop.nuspec release notes 
            var testCopAssembly = typeof(TestCopJumpToTestFileAction).Assembly;
            string testcopAssemblyVersion = testCopAssembly.GetName().Version.ToString();

            string nuspecFilePath = Path.GetDirectoryName(testCopAssembly.Location) + Path.DirectorySeparatorChar + "TestCop.nuspec";
            var doc = new XmlDocument();
            doc.Load(nuspecFilePath);

            var releaseNotes = doc.SelectSingleNode("//*[local-name()='releaseNotes']").InnerText;
            releaseNotes = releaseNotes.Replace("\n\r","").Replace("\t", " ").Trim();
            var firstVersionInNotes = releaseNotes.Split(' ').First();

            firstVersionInNotes = firstVersionInNotes.Replace("-EAP", "");

            Assert.AreEqual(testcopAssemblyVersion, firstVersionInNotes);
        }
    }
}
