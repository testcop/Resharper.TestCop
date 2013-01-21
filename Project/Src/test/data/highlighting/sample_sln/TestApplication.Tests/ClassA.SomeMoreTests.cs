using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCop.TestApplication.Tests
{
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
