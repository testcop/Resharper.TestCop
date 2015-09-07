using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Application;
using JetBrains.Application.BuildScript.Application;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.Application.BuildScript.PackageSpecification;
using JetBrains.Application.BuildScript.Solution;
using JetBrains.Application.Environment;
using JetBrains.Application.Environment.HostParameters;
using JetBrains.Metadata.Reader.API;
using JetBrains.Metadata.Utils;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using JetBrains.TestFramework.Utils;
using JetBrains.Util;
using NUnit.Framework;

[assembly: RequiresSTA]

namespace TestCop.Plugin.Tests
{    
    [ZoneDefinition]
    public interface ITestCopTestZone : ITestsZone, IRequire<PsiFeatureTestZone>
    {}

    [SetUpFixture]
    public class TestEnvironmentAssembly : ExtensionTestEnvironmentAssembly<ITestCopTestZone>
    {
    }

}
