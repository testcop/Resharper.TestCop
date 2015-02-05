// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2013
// --

using System.Text.RegularExpressions;
using NUnit.Framework;
using TestCop.Plugin.Extensions;

namespace TestCop.Plugin.Tests.Extensions
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void StringInTests()
        {
            Assert.IsTrue("ABC".In("C", "B", "ABC" ));
            Assert.IsFalse("ABC".In("C", "B", "a" ));
        }

        [Test]
        public void StringEndsWithTests()
        {
            Assert.IsTrue("ABC".EndsWith( new[] {"C", "X", "Y"}));
            Assert.IsFalse("ABC".EndsWith( new[] {"A", "B", "Y"}));            
        }

        [Test]
        public void StartsWithTests()
        {
            Assert.IsTrue("ABC".StartsWith(new[]{"C", "B", "A"}));
            Assert.IsFalse("ABC".StartsWith(new[] { "C", "B", "a" }));
        }

        [Test]
        public void RemoveTrailingTests()
        {
            Assert.AreEqual("", "".RemoveTrailing("C"));
            Assert.AreEqual("AB", "ABC".RemoveTrailing("C"));
            Assert.AreEqual("A", "ABC".RemoveTrailing("BC"));
            Assert.AreEqual("ABC", "ABC".RemoveTrailing("A"));            
        }
 
        [Test]
        public void FlipTests()
        {
            Assert.AreEqual("A.Test", "A.Test".Flip(false,".Test"));
            Assert.AreEqual("A", "A.Test".Flip(true,".Test"));

            Assert.AreEqual("A.X", "A.X".Flip(true, ".Test"));
            Assert.AreEqual("A.X.Test", "A.X".Flip(false, ".Test"));
        }

        [Test]
        public void AppendIfNotNullTests()
        {            
            Assert.AreEqual("A|B", "A".AppendIfNotNull("|","B"));
            Assert.AreEqual("A", "A".AppendIfNotNull("|", string.Empty));
        }

        [Test]
        public void AppendIfMissingTests()
        {
            Assert.AreEqual("AB", "A".AppendIfMissing("B"));
            Assert.AreEqual("AB", "AB".AppendIfMissing("B"));            
        }

        [Test]
        public void RemoveLeadingTests()
        {
            Assert.AreEqual("AB", "AB".RemoveLeading("X"));
            Assert.AreEqual("", "AB".RemoveLeading("AB"));
            Assert.AreEqual("B", "AB".RemoveLeading("A"));

            Assert.AreEqual("B", "AB".RemoveLeading(new []{"A","B"}));
        }
         
    }
}