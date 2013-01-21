using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCop.TestApplication.Tests.Samples
{
    /* USE CASE: 
     * Test file has the wrong namespace for the code under test 
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
