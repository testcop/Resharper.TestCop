using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestCop.TestApplication.NS1;

namespace TestCop.TestApplication.Tests
{
    /* USE CASE: 
     * Test file has the wrong namespace for the code under test 
     */
    [TestClass]
    public class ClassDTests
    {
        [TestMethod]
        public void ReturnsTrueMethodTest()
        {
            Assert.IsTrue(new ClassD().ReturnsTrue());
        }
    }
}
