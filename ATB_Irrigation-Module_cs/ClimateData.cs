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
using System.IO;
using System.Reflection;

using local;

namespace atbApi
{
    namespace data
    {

        /*! Values that represent different time steps. */
        public enum TimeStep
        {
            /*! hourly timeStep */
            hour = 0x0,
            /*! daily timeStep */
            day = 0x1,
            /*! monthly timeStep */
            month = 0x2,
        }


        /*!
        * \brief	defines a collection of climate values.
        *
        * \remarks	if "mean_temp" is ommitted, <c>max_temp + min_temp / 2</c> is used.
        */

        public class ClimateValues : BaseValues
        {
            /*! maximum temperature [°C]. */
            public Double? max_temp { get; set; }
            /*! minimum temperature [°C]. */
            public Double? min_temp { get; set; }
            /*! mean temperature [°C]. */
            public Double? mean_temp { get; set; }
            /*! relative humidity [%]. */
            public Double? humidity { get; set; }
            /*! windspeed at 2m height "u2" [m/s] */
            public Double? windspeed { get; set; }
            /*! duration of sunshine [h]. */
            public Double? sunshine_duration { get; set; }
            /*! global radiation [MJ/(m²*d)]. */
            public Double? Rs { get; set; }
            /*! precipitation [mm]. */
            public Double? precipitation { get; set; }


            public new void parseData(IDictionary<String, String> values)
            {
                base.parseData(values);
            }
        }

        /*!
         * \brief   Aggregated climate data for a calculation of a cropsequence: The data must begin at first
         * seed date and must not end before last harvest date.
         *
         */

        public class Climate
        {
            /*! vector with data. */
            private IDictionary<DateTime, ClimateValues> climateData = new Dictionary<DateTime, ClimateValues>();
            /*! incremental step of data vector. */
            public TimeStep step { get; set; }

            public Climate()
            {
                this.step = TimeStep.month;
            }

            public Climate(TimeStep step)
            {
                this.step = step;
            }

            /*!
             * \brief   Get values for a given date.
             *
             * \param   date The date to get data for. It is adjusted to next lower bound of the timestep
             *
             * \return  The values if available for requested date, else null is returned.
             */

            public ClimateValues getValues(DateTime date)
            {
                ClimateValues resultSet;
                climateData.TryGetValue(Tools.AdjustTimeStep(date, step), out resultSet);
                return resultSet;
            }
        }
    }
}
