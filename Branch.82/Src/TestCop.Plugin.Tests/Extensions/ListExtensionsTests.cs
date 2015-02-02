// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System;
using System.Collections.Generic;
using NUnit.Framework;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Tests.Extensions
{
    [TestFixture]
    public class ListExtensionsTests
    {
        [Test]
        public void AddIfMissingTest()
        {
            var listOfStrings = new List<string>();

            listOfStrings.AddIfMissing("AA");
            listOfStrings.AddIfMissing("AA");
            listOfStrings.AddIfMissing("AA");
            listOfStrings.AddIfMissing("BB");
            listOfStrings.AddIfMissing("BB");

            Assert.AreEqual(2, listOfStrings.Count);
            Assert.IsTrue(listOfStrings.Contains("AA"));
            Assert.IsTrue(listOfStrings.Contains("BB"));            
        }

        [Test]
        public void AddIfMissingUsingMatcherTest()
        {
            Func<string, string, bool> matcher = (s1, s2) => s1.Substring(0, 1) == s2.Substring(0, 1);
            var listOfStrings = new List<string>();
            
            listOfStrings.AddIfMissing("AA", matcher);
            listOfStrings.AddIfMissing("AA", matcher);
            listOfStrings.AddIfMissing("AA", matcher);
            listOfStrings.AddIfMissing("A", matcher);
            listOfStrings.AddIfMissing("A", matcher);

            Assert.AreEqual(1, listOfStrings.Count);            
            Assert.IsTrue(listOfStrings.Contains("AA"));
        }

        [Test]
        public void AddIfMissingWithRangeTest()
        {
            Func<string, string, bool> matcher = (s1, s2) => s1.Substring(0, 1) == s2.Substring(0, 1);
            var stringsToAdd = new List<string> { "AA", "AA", "A", "A" };
            var listOfStrings = new List<string>();

            listOfStrings.AddRangeIfMissing(stringsToAdd, matcher);

            Assert.AreEqual(1, listOfStrings.Count);
            Assert.IsTrue(listOfStrings.Contains("AA"));  
        }        
    }
}