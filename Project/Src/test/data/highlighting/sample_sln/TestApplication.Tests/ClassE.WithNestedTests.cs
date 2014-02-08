using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestCop.TestApplication.NS1;

namespace TestCop.TestApplication.Tests
{
    /* USE CASE: 
     * Nested test classes are not checked by TestCop
     */
    [TestClass]
    public class ClassEWithNestedTests
    {
        [TestMethod]
        public void ReturnsTrueMethodTest()
        {
            Assert.IsTrue(new ClassD().ReturnsTrue());
        }
        
        [TestClass]
        public class NestedTests
        {
        }
    }
}
