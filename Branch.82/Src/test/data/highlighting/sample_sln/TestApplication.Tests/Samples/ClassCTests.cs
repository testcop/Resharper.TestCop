using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCop.TestApplication.Tests.Samples
{
    /* USE CASE: 
        * Test file has the wrong namespace for the code under test
        * and testcop should offer to create in correct location 
        * or open the code file in the wrong location        
        */
    [TestClass]
    public class ClassCTests
    {
        [TestMethod]
        public void ReturnsTrueMethodTest()
        {
            Assert.IsTrue(new ClassC().ReturnsTrue());
        }
    }
}
