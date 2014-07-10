using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestCop.TestApplication2;

namespace TestCop.TestApplication2Tests
{
    /* test to confirm that we can have a custom testing namespace suffix */
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
