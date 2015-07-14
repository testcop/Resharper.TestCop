// --
// -- TestCop http://testcop.codeplex.com
// -- License http://testcop.codeplex.com/license
// -- Copyright 2015
// --

using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.ActionManagement;
using JetBrains.Application.PluginSupport;
using JetBrains.ReSharper.Daemon;
using TestCop.Plugin.Highlighting;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("TestCop")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("TestCop")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyCopyright("Copyright 2015")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("79794f65-47a0-4b60-9356-5246c6530bbf")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.8.3.0")]
[assembly: AssemblyFileVersion("1.8.3.0")]

// The following information is displayed by ReSharper in the Plugins dialog
[assembly: PluginTitle("TestCop Resharper Plugin")]
[assembly: PluginDescription("Helps jump between test and code with some useful naming conformance checks")]
[assembly: PluginVendor("")]

//http://confluence.jetbrains.net/display/ReSharper/2.3+Actions+and+Menu+Items+%28R7%29
[assembly: ActionsXml("TestCop.Plugin.Resources.Actions.xml")]

[assembly: RegisterStaticHighlightingsGroup(Highlighter.HighlightingGroup, "Testing", true)]
[assembly: RegisterConfigurableHighlightingsGroupAttribute("Testing", "Testing")]

[assembly: RegisterConfigurableSeverity(
        MethodShouldBePublicWarning.SeverityId,
        null, "Testing",
        "Test method should be public",
        "TestCop : Method with testing attributes should be public",
        Severity.ERROR,
        false)]

[assembly: RegisterConfigurableSeverity(
        ClassShouldBePublicWarning.SeverityId,
        null, "Testing",
        "Test class should be public",
        "TestCop : Class with testing attributes should be public",
        Severity.ERROR,
        false)]

[assembly: RegisterConfigurableSeverity(
        TestClassNameDoesNotMatchFileNameWarning.SeverityId,
        null, "Testing",
        "Test class name should match file name",
        "TestCop : The name of the test file should match the test class name it contains",
        Severity.ERROR,
        false)]

[assembly: RegisterConfigurableSeverity(
        TestClassNameSuffixWarning.SeverityId,
        null, "Testing",
        "All test classes should have the same suffix",
        "TestCop : To easily identify a test class by its name it must have the configured suffix",
        Severity.ERROR,
        false)]

[assembly: RegisterConfigurableSeverity(
        TestMethodMissingCodeWarning.SeverityId,
        null, "Testing",
        "Test methods should contain code",
        "TestCop : All tests methods should test something",
        Severity.ERROR,
        false)]

[assembly: RegisterConfigurableSeverity(
        FilesNotPartOfProjectWarning.SeverityId,
        null, "Testing",
        "Orphaned file not part of project",
        "TestCop : All code files should be part of project",
        Severity.ERROR,
        false)]
    