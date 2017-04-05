/*!
 * \file	version.cs
 *
 * \brief	declares automatic (compile time) versioning macros
 *   
 * \author  Hunstock
 * \date    23.07.2015
 */

using System;

/*! 
 * \brief   namespace for automatic versioning macros and classes
 * 
 */

namespace version
{
    /*!
     * \brief   class to hold information about the version.
     *
     */

    public static class versionInfo
    {
        public const String VERSION_MAJOR = "1";
        public const String VERSION_MINOR = "0";
        public const String VERSION_REVISION = "1";
        public const String VERSION_BUILD = "629";
        public const String VER_FILE_VERSION_STR = VERSION_MAJOR + "." + VERSION_MINOR + "." + VERSION_REVISION + "." + VERSION_BUILD;

#if _WIN64
        public const String VER_FILE_DESCRIPTION_STR = "ATB Irrigation Module for MIKE-Basin Software to calculate transpiration, evaporation, evapotranspiration and irrigation demand for agriculture. (64bit)";
        public const String VER_PRODUCTNAME_STR = "ATB Irrigation Module for MIKE-Basin Software by DHI (64bit)";
#else
        public const String VER_FILE_DESCRIPTION_STR = "ATB Irrigation Module for MIKE-Basin Software to calculate transpiration, evaporation, evapotranspiration and irrigation demand for agriculture. (x86)";
        public const String VER_PRODUCTNAME_STR = "ATB Irrigation Module for MIKE-Basin Software by DHI (x86)";
#endif
        public const String VER_PRODUCT_VERSION_STR = "ATB_Irrigation-Module v" + VER_FILE_VERSION_STR;
        public const String VER_TITLE = "ATB_Irrigation-Module";
        public const String VER_ORIGINAL_FILENAME_STR = "ATB_Irrigation-Module.dll";
        public const String VER_INTERNAL_NAME_STR = "ATB_Irrigation-Module_cs.dll";
        public const String VER_COPYRIGHT_STR = "© 2015 Leibniz-Institut für Agrartechnik Potsdam-Bornim e.V.";
        public const String VER_COMPANY_STR = "Leibniz-Institut für Agrartechnik Potsdam-Bornim e.V.";

#if DEBUG
        public const String VER_CONFIGURATION_STR = "DEBUG";
#else
        public const String VER_CONFIGURATION_STR = "RELEASE";
#endif
    }
}
