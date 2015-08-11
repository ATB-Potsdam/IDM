using System.Resources;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using version;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(versionInfo.VER_TITLE)]
[assembly: AssemblyDescription(versionInfo.VER_FILE_DESCRIPTION_STR)]
[assembly: AssemblyConfiguration(versionInfo.VER_CONFIGURATION_STR)]
[assembly: AssemblyCompany(versionInfo.VER_COMPANY_STR)]
[assembly: AssemblyProduct(versionInfo.VER_PRODUCT_VERSION_STR)]
[assembly: AssemblyCopyright(versionInfo.VER_COPYRIGHT_STR)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

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
[assembly: AssemblyVersion(versionInfo.VER_FILE_VERSION_STR)]
[assembly: AssemblyFileVersion(versionInfo.VER_FILE_VERSION_STR)]
