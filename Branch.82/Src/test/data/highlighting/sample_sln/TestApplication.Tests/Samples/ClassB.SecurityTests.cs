using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCop.TestApplication.Tests.Samples
{
    /* USE CASE
     * Class under test (ClassB) doesn't exist within associated code assembly
     * so TestCop should highlight issue and offer to create the missing code file
     * taking into account the subnamespace of 'Samples'
     */
    [TestClass]
    public class ClassBSecurityTests
    {
    }
}
