using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

[assembly: RequiresSTA]

namespace TestCop.Plugin.Tests
{
    [ZoneDefinition]
    public interface ITestCopTestZone : ITestsEnvZone, IRequire<PsiFeatureTestZone>
    { }

    [SetUpFixture]
    public class TestEnvironmentAssembly : ExtensionTestEnvironmentAssembly<ITestCopTestZone>
    {
    }
}
