/*!
* \file		ClimateData.cs
*
* \brief	Class file for climate data types and access
* \author	Hunstock
* \date	09.07.2015
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace atbApi
{
    namespace data
    {
        class ClimateData
        {
            public enum timeStep
            {
                /*! hourly timeStep */
                hour = 0x0,
                /*! daily timeStep */
                day = 0x1,
                /*! monthly timeStep */
                month = 0x2,
            };

            /*!
            * \brief	defines a collection of climate values.
            *
            * \remarks	if "mean_temp" is ommitted, <c>max_temp + min_temp / 2</c> is used.
            */

            public struct climateValues
            {
                /*! maximum temperature [°C]. */
                double max_temp;
                /*! minimum temperature [°C]. */
                double min_temp;
                /*! mean temperature [°C]. */
                double mean_temp;
                /*! relative humidity [%]. */
                double humidity;
                /*! windspeed at 2m height "u2" [m/s]. */
                double windspeed;
                /*! duration of sunshine [h]. */
                double sunshine_duration;
                /*! global radiation [MJ/(m²*d)]. */
                double Rs;
                /*! precipitation [mm]. */
                double precipitation;
            };

            /*!
             * \brief   Aggregated climate data for a calculation of a cropsequence: The data must begin at first
             * seed date and must not end before last harvest date.
             *
             */

            public struct climateData
            {
                /*! start date of data. */
                DateTime start;
                /*! incremental step of data vector. */
                timeStep step;
                /*! vector with data. */
                List<climateValues> data;
            };
        }
    }
}
