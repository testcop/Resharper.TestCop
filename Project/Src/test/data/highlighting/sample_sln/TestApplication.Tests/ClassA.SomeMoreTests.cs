using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCop.TestApplication.Tests
{
    /* USE CASE: 
   * Test file is the named correctly but uses a '.' in the file name
   * Testcop should offer to switch to ClassA
   */
    [TestClass]
    public class ClassASomeMoreTests
    {
        [TestMethod]
        public void ReturnsTrueMethodTest()
        {
            Assert.IsTrue(new ClassA().ReturnsTrue());
        }
    }
}

