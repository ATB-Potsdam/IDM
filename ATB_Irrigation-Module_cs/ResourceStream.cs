﻿/*!
 * \file    ResourceStream.cs
 *
 * \brief   Implements the resource stream class to read compiled in resources like the internal plant and soil databases.
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
        private static string[] resourceNames = typeof(atbApi.Transpiration).GetTypeInfo().Assembly.GetManifestResourceNames();

        internal static bool ResourceExists(String resourceName)
        {
            return Array.IndexOf(resourceNames, resourceName) >= 0;
        }

        internal static Stream GetResourceStream(String resourceName)
        {
            //use this for .net 4.0
            //Assembly assembly = Assembly.GetExecutingAssembly(); 
            //use this for .net 4.5
            Assembly assembly = typeof(atbApi.Transpiration).GetTypeInfo().Assembly; 

            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}
