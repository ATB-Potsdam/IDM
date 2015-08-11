/*!
* \file		plantData.cs
*
* \brief	Class file for plant parameter types and access
* \author	Hunstock
* \date     09.07.2015
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.IO;

using local;

namespace atbApi
{
    namespace data
    {

        public static class PlantDb
        {
            private static String plantDataResource = "local.IWRM_ATB-PlantData.csv";
            
            public static Stream GetResourceStream() {
                Assembly assembly = Assembly.GetExecutingAssembly();

                return assembly.GetManifestResourceStream("local.IWRM_ATB-PlantData.csv");
            }
            
            public static ICollection<String> GetPlantNames()
            {
                ICollection<String> names = new HashSet<String>();

                CsvReader csvReader = new CsvReader(ResourceStream.GetResourceStream(plantDataResource));
                while (!csvReader.EndOfStream())
                {
                    IDictionary<String, String> fields = csvReader.readLine();
                    names.Add(fields["dataObjName"]);
                }
                return names;
            }
        }

        /*!
        * \brief	enum for the different plant stages.
        *
        * \remarks	standard are 4 stages, for winter crops, the development stage is divided into 3
        * pieces to better fit the development stagnation during frost.
        */

        public enum plantStage
        {
            /*! initial stage */
            initial = 0x0,
            /*! development stage */
            development = 0x1,
            /*! development stagnation stage for 6-stage parameters */
            development_stagnation = 0x2,
            /*! development finish stage for 6-stage parameters */
            development_finish = 0x3,
            /*! middle season stage */
            mid_season = 0x4,
            /*! late season stage */
            late_season = 0x5,
        };

        /*!
        * \brief	defines a collection of plant parameters.
        *
        */
        public class plantValues
        {
            /*! plant stage */
            public plantStage? stage { get; set; }
            /*! crop coefficient */
            public double? Kc { get; set; }
            /*! basal crop coefficient */
            public double? Kcb { get; set; }
            /*! leaf area index */
            public double? LAI { get; set; }
            /*! rooting depth without irrigation */
            public double? Zr { get; set; }
            /*! rooting depth irrigated */
            public double? Zr_irrigated { get; set; }
            /*! depletion fraction */
            public double? p { get; set; }
            /*! yield response to water stress */
            public double? Ky { get; set; }
            /*! yield response for all stages */
            public double? Ky_total { get; set; }
            /*! plant height */
            public double? height { get; set; }
            /*! true if this object is a fallow */
            public bool? isFallow { get; set; }
            /*! botanical name  */
            public String botanicalName { get; set; }
            /*! german name */
            public String germanName { get; set; }
            /*! english name */
            public String englishName { get; set; }
        };


        /*!
         * \brief
         *
         * \author  Hunstock
         * \date    11.08.2015
         */
        public class Plant
        {
            private IDictionary<int, plantValues> plantData = new Dictionary<int, plantValues>();
            public String name { get; set; }
            public int stageTotal { get; set; }

            public Plant(String name)
                : this(name, PlantDb.GetResourceStream())
            {
            }

            public Plant(String name, Stream stream)
            {
                if (name == null) return;

                this.name = name;
                CsvReader csvReader = new CsvReader(stream);
                while (!csvReader.EndOfStream())
                {
                    IDictionary<String, String> fields = csvReader.readLine();
                    if (!name.Equals(fields["dataObjName"])) continue;

                    plantValues values = new plantValues();
                    plantStage stage;
                    if (Enum.TryParse(fields["name"], out stage)) values.stage = stage;
                    if (fields["Kc"] != "") values.Kc = Double.Parse(fields["Kc"], CultureInfo.InvariantCulture);
                    if (fields["Kcb"] != "") values.Kcb = Double.Parse(fields["Kcb"], CultureInfo.InvariantCulture);
                    if (fields["LAI"] != "") values.LAI = Double.Parse(fields["LAI"], CultureInfo.InvariantCulture);
                    if (fields["Zr"] != "") values.Zr = Double.Parse(fields["Zr"], CultureInfo.InvariantCulture);
                    if (fields["Zr_irrigated"] != "") values.Zr_irrigated = Double.Parse(fields["Zr_irrigated"], CultureInfo.InvariantCulture);
                    if (fields["p"] != "") values.p = Double.Parse(fields["p"], CultureInfo.InvariantCulture);
                    if (fields["Ky"] != "") values.Ky = Double.Parse(fields["Ky"], CultureInfo.InvariantCulture);
                    if (fields["Ky_total"] != "") values.Ky_total = Double.Parse(fields["Ky_total"], CultureInfo.InvariantCulture);
                    if (fields["height"] != "") values.height = Double.Parse(fields["height"], CultureInfo.InvariantCulture);
                    if (fields["isFallow"] != "") values.isFallow = Boolean.Parse(fields["isFallow"]);
                    if (fields["botanicalName"] != "") values.botanicalName = fields["botanicalName"];
                    if (fields["germanName"] != "") values.germanName = fields["germanName"];
                    if (fields["englishName"] != "") values.englishName = fields["englishName"];
                    int _iterator = Int32.Parse(fields["_iterator"], CultureInfo.InvariantCulture);
                    plantData.Add(_iterator, values);
                    this.stageTotal = Math.Max(this.stageTotal, _iterator);
                }
            }

            public plantValues getSet(int iterator)
            {
                plantValues defaultSet;
                plantValues resultSet;
                plantData.TryGetValue(0, out defaultSet);
                plantData.TryGetValue(iterator, out resultSet);

                return Tools.MergeObjects<plantValues>(defaultSet, resultSet);
            }
        }
    }
}
