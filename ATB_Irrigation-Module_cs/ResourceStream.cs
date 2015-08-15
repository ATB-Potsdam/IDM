/*!
 * \file    ResourceStream.cs
 *
 * \brief   Implements the resource stream class to read compiled in resources.
 *
 * \author	Hunstock
 * \date     11.08.2015
 */

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
