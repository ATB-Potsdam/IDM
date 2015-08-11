using System;
using System.IO;
using System.Reflection;

namespace local
{
    public static class ResourceStream
    {
        public static Stream GetResourceStream(String resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}
