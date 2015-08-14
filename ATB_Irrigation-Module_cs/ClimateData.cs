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
        public static class ClimateDb
        {
            private static String climateDataResource = "local.IWRM_ATB-PlantData.csv";
            
            public static Stream GetResourceStream() {
                Assembly assembly = Assembly.GetExecutingAssembly();

                return assembly.GetManifestResourceStream("local.IWRM_ATB-PlantData.csv");
            }
            
            public static ICollection<String> GetPlantNames()
            {
                ICollection<String> names = new HashSet<String>();

                CsvReader csvReader = new CsvReader(ResourceStream.GetResourceStream(climateDataResource));
                while (!csvReader.EndOfStream())
                {
                    IDictionary<String, String> fields = csvReader.readLine();
                    names.Add(fields["dataObjName"]);
                }
                return names;
            }
        }
        public enum TimeStep
        {
            /*! hourly timeStep */
            hour = 0x0,
            /*! daily timeStep */
            day = 0x1,
            /*! monthly timeStep */
            month = 0x2,
        };

        public class ClimateValues : BaseValues
        {
            /*!
            * \brief	defines a collection of climate values.
            *
            * \remarks	if "mean_temp" is ommitted, <c>max_temp + min_temp / 2</c> is used.
            */

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
        };

        /*!
         * \brief   Aggregated climate data for a calculation of a cropsequence: The data must begin at first
         * seed date and must not end before last harvest date.
         *
         */

        public class Climate
        {
            /*! vector with data. */
            private IDictionary<DateTime, ClimateValues> climateData = new Dictionary<DateTime, ClimateValues>();
            /*! start date of data. */
            public DateTime start { get; set; }
            /*! incremental step of data vector. */
            public TimeStep step { get; set; }

                        public Climate(String name)
                : this(name, ClimateDb.GetResourceStream())
            {
            }

  public Climate(String name, Stream stream)
            {
/*
                if (name == null) return;

                this.name = name;
                CsvReader csvReader = new CsvReader(stream);
                while (!csvReader.EndOfStream())
                {
                    IDictionary<String, String> fields = csvReader.readLine();
                    if (!name.Equals(fields["dataObjName"])) continue;

                    PlantValues values = new PlantValues();
                    values.parseData(fields);
                    int _iterator = Int32.Parse(fields["_iterator"], CultureInfo.InvariantCulture);
                    plantData.Add(_iterator, values);
                    this.stageTotal = Math.Max(this.stageTotal, _iterator);
                }
*/
            }

            /*
        public ClimateValues getSet(int iterator)
        {
            PlantValues defaultSet;
            PlantValues resultSet;
            plantData.TryGetValue(0, out defaultSet);
            plantData.TryGetValue(iterator, out resultSet);

            return Tools.MergeObjects<PlantValues>(defaultSet, resultSet);
        }
             */

        };
    }
}
