﻿using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.ActionManagement;
using JetBrains.Application.PluginSupport;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("TestCop")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("TestCop")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

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
[assembly: AssemblyVersion("0.0.0.2")]
[assembly: AssemblyFileVersion("0.0.0.2")]

// The following information is displayed by ReSharper in the Plugins dialog
[assembly: PluginTitle("TestCop Resharper Plugin")]
[assembly: PluginDescription("Helps jump between test and code with some useful naming conformance checks")]
[assembly: PluginVendor("")]

//http://confluence.jetbrains.net/display/ReSharper/2.3+Actions+and+Menu+Items+%28R7%29
[assembly: ActionsXml("TestCop.Plugin.Resources.Actions.xml")]
