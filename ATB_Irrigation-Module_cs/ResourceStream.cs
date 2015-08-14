using System;
using System.IO;
using System.Reflection;

namespace local
{
    internal static class ResourceStream
    {
        internal static Stream GetResourceStream(String resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}
