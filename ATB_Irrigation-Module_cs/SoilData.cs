/*!
 * \file		SoilData.cs
 *
 * \brief	Class file for soil database, parameter types and data access
 *
 * \author	Hunstock
 * \date     09.07.2015
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

using local;

namespace atbApi
{
    namespace data
    {

        /*!
         * \brief   Soil values to track for continuous calculation without gap in the water balance.
         *
         */

        public class SoilConditions
        {
            /*! drainage in the root zone */
            public double drRz { get; set; }
            /*! totally available water in the root zone */
            public Double? tawRz { get; set; }
            /*! electrical conductivity in the root zone */
            public Double? ecRz { get; set; }
            /*! drainage in the deep zone (below root zone up to 2m depth) */
            public double drDz { get; set; }
            /*! totally available water in the deep zone (below root zone up to 2m depth) */
            public Double? tawDz { get; set; }
            /*! electrical conductivity in the deep zone (below root zone up to 2m depth) */
            public Double? ecDz { get; set; }
            /*! drainage in the evaporation layer */
            public double de { get; set; }
            /*! percolation from the evaporation layer */
            public double dpe { get; set; }
            /*! rooting depth of the crop */
            public double zr { get; set; }

            /*!
             * \brief   Default constructor. Values are set to defaults.
             *
             */

            public SoilConditions(
                double drRz = 0.4,
                double drDz = 0.3,
                double de = 0.4,
                double dpe = 0.0,
                double zr = 0.3,
                Double? tawRz = null,
                Double? tawDz = null,
                Double? ecRz = null,
                Double? ecDz = null
            )
            {
                this.drRz = drRz;
                this.drDz = drDz;
                this.de = de;
                this.dpe = dpe;
                this.zr = zr;
                this.tawRz = tawRz;
                this.tawDz = tawDz;
                this.ecRz = ecRz;
                this.ecDz = ecDz;
            }

            /*!
             * \brief   Clone constructor.
             *
             * \param   soilConditions  The soil conditions to clone.
             */

            public SoilConditions(SoilConditions soilConditions)
            {
                //use this for .net 4.0
                //foreach (PropertyInfo pi in this.GetType().GetProperties())
                //use this for .net 4.5
                foreach (PropertyInfo pi in this.GetType().GetRuntimeProperties())
                {
                    if (soilConditions.GetType().GetRuntimeProperty(pi.Name) == null) continue;
                    pi.SetValue(this, soilConditions.GetType().GetRuntimeProperty(pi.Name).GetValue(soilConditions, null), null);
                }
            }

            /*!
             * \brief   Constructor with soil given to get taw from soil.
             *
             * \param   soil        The soil.
             * \param   zr          The initial root depth.
             * \param   depletionRz The initial depletion in the root zone.
             * \param   depletionDz The initial depletion in the zone under root up to 2m.
             * \param   depletionDe The initial depletion in the evaporation layer.
             */

            public SoilConditions(Soil soil, double zr = 0.3, double depletionRz = 0.4, double depletionDz = 0.3, double depletionDe = 0.4)
            {
                //hardcoded default values if values are ommitted in constructor
                var zeInitial = 0.1;

                this.zr = Math.Max(0, Math.Min(soil.maxDepth, zr));
                var soilSetStart = soil.getValues(this.zr);
                var soilSetMax = soil.getValues(soil.maxDepth);
                tawRz= 1000 * (soilSetStart.Qfc - soilSetStart.Qwp) * this.zr;
                var tawMax= 1000 * (soilSetMax.Qfc - soilSetMax.Qwp) * soil.maxDepth;
                tawDz= tawMax - tawRz;
                var ze= soilSetStart.Ze != null ? (double)soilSetStart.Ze : zeInitial;

                drRz = depletionRz * (double)tawRz;
                drDz = depletionDz * (double)tawDz;
                de = depletionDe * (double)tawRz * (ze / this.zr);
            }
        }

        /*!
         * \brief   Static class with useful functions to adjust soil conditions for changed rooting depth or exceeded depletion.
         *
         */

        internal static class SoilConditionTools
        {
            /*!
             * \brief   adjust drainage according to changed root depth.
             *
             * \param   lastConditions  The last soil conditions.
             * \param   tawRz           The totally available water in root zone.
             * \param   tawDz           The totally available water in the zone under root up to 2m or maxDepth.
             * \param   zr              The rooting depth.
             * \param   maxDepth        The maximum depth of the soil profile.
             *
             * \return  The fixed SoilConditions.
             */

            internal static SoilConditions AdjustSoilConditionsZr(SoilConditions lastConditions, double tawRz, double tawDz, double zr, double maxDepth)
            {
                if (lastConditions.tawRz == null) lastConditions.tawRz = tawRz;
                if (lastConditions.tawDz == null) lastConditions.tawDz = tawDz;

                if (zr == lastConditions.zr) return lastConditions;

                var drSum = lastConditions.drRz + lastConditions.drDz;

                //catch special case root zone from max to 0
                if (lastConditions.zr == maxDepth && zr == 0) {
                    lastConditions.drRz= 0;
                    lastConditions.drDz= lastConditions.drRz;
                }
                //catch special case root zone from 0 to max
                else if (lastConditions.zr == 0 && zr == maxDepth) {
                    lastConditions.drRz= lastConditions.drDz;
                    lastConditions.drDz= 0;
                }
                //root zone shrinks -> add root drainage to deep zone
                else if (zr < lastConditions.zr) {
                    var tawRzRatio= ((double)lastConditions.tawRz / lastConditions.zr) / (tawRz / zr);
                    var zrFactor= zr / lastConditions.zr;
                    lastConditions.drRz= lastConditions.drRz * zrFactor / tawRzRatio;
                    lastConditions.drDz= drSum - lastConditions.drRz;
                }
                //root zone grows -> add deep drainage to root zone
                else {
                    var tawDzRatio= ((double)lastConditions.tawDz / (maxDepth - lastConditions.zr)) / (tawDz / (maxDepth - zr));
                    var zrFactor= (maxDepth - zr) / (maxDepth - lastConditions.zr);
                    lastConditions.drDz= lastConditions.drDz * zrFactor / tawDzRatio;
                    lastConditions.drRz= drSum - lastConditions.drDz;
                }
                lastConditions.tawRz = tawRz;
                lastConditions.tawDz = tawDz;

                return lastConditions;
            }


            /*!
             * \brief   adjust soil conditions if drainage is exceeded and the water content is negative.
             *
             * \param   lastConditions  The last soil conditions.
             * \param   tawRz           The totally available water in root zone.
             * \param   tawDz           The totally available water in the zone under root up to 2m or maxDepth.
             * \param   zrNew           The new rooting depth.
             * \param   maxDepth        The maximum depth of the soil profile.
             * \param [in,out]  e       The actual evaporation, may be adjusted.
             * \param [in,out]  t       The actual transpiration, may be adjusted.
             *
             * \return  The fixed SoilConditions.
             */

            internal static SoilConditions AdjustSoilConditionsExceeded(SoilConditions lastConditions, double tawRz, double tawDz, double zrNew, double maxDepth, ref double e, ref double t)
            {
                if (zrNew != lastConditions.zr) lastConditions = AdjustSoilConditionsZr(lastConditions, tawRz, tawDz, zrNew, maxDepth);

                var balanceSum = e + t;
                //calculate soil water balance
                var dpRz= Math.Max(0, balanceSum - lastConditions.drRz);
                var drRz= lastConditions.drRz - balanceSum + dpRz;

                if (drRz < 0) {
                    //negative drainage should not happen -> percolate excess water to deep zone
                    dpRz -= drRz;
                    drRz= 0;
                }
                else if (drRz > lastConditions.tawRz) {
                    //drainage exceeds taw -> adjust this day values to stay beyond this limit
                    var drRzExceeded= drRz - (double)lastConditions.tawRz;

                    var eFactor= e / balanceSum;
                    var etFactor= t / balanceSum;
                    t = Math.Min(0, t - drRzExceeded * etFactor);
                    e = Math.Min(0, e - drRzExceeded * eFactor);
                    drRz = lastConditions.drRz - balanceSum - drRzExceeded + dpRz;
                }

                var dpDz= Math.Max(0, dpRz - lastConditions.drDz);
                var drDz= lastConditions.drDz - dpRz + dpDz;

                if (drDz < 0) {
                    //negative drainage should not happen -> deep percolate excess water
                    dpDz -= drDz;
                    drDz= 0;
                }
                if (drDz > tawDz) {
                    //drainage exceeds taw should not happen in deep zone -> write exceed value to result table
                    drDz= tawDz;
                }

                return lastConditions;
            }

        }

        
        /*!
        * \brief	defines a collection of soil values.
        *
        */

        public class SoilValues : BaseValues
        {
            /*! field capacity [m³/m³] or unitless */
            public double Qfc { get; set; }
            /*! wilting point [m³/m³] or unitless */
            public double Qwp { get; set; }
            /*! depth of evaporation layer [m] */
            public double? Ze { get; set; }
            /*! humus content [%] */
            public double humus { get; set; }

            private static IDictionary<String, String> propertyMapper = new Dictionary<String, String>();

            /*!
             * \brief   Default static constructor.
             *
             */

            static SoilValues()
            {
                // propertyMapper.Add("qfc", "fieldCapacity");
            }

            public void parseData(IDictionary<String, String> values)
            {
                this.Ze = 0.1;
                base.parseData(values, propertyMapper);
            }
        }

        /*!
         * \brief   The soil database class.
         * 
         * \remarks This class and its public contructor is used to load an external csv-File as user defined soil database.
         *          Because of this portable dll cannot read files itself, a stream instance must be provided to read data from.
         *          The stream must be closed in the calling instance.
         *
         */

        public class SoilDb
        {
            private const double DepthOffset = 0.000000000001;
            private Stream soilDbFileStream;
            private IDictionary<String, Soil> soils = new Dictionary<String, Soil>();
            private ICollection<String> names = new HashSet<String>();
            private IDictionary<String, double> maxDepth = new Dictionary<String, double>();
            private IDictionary<String, IDictionary<Double, SoilValues>> soilData = new Dictionary<String, IDictionary<Double, SoilValues>>();

            /*!
             * \brief   Constructor to load external csv-File as soil database.
             *
             * \param   soilDbFileStream   A file stream to read csv data from.
             *
             * \code{.unparsed}
             * 'create filestream for csv data file
             * Dim fs As System.IO.FileStream = New IO.FileStream("C:\DHI\MIKE-Basin_SoilData.csv", IO.FileMode.Open)
             * 'create new soilDb
             * Dim soilDb As atbApi.data.SoilDb = New atbApi.data.SoilDb(fs)
             * 'important: close fileStream after SoilDb is created
             * fs.Close()
             * 'create new soil and provide your own soilDb to constructor
             * Dim mySoil As atbApi.data.Soil = New atbApi.data.Soil("soilname_from_csv_data", soilDb)
             * \endcode
             */

            public SoilDb(Stream soilDbFileStream)
            {
                this.soilDbFileStream = soilDbFileStream;
                CsvReader csvReader = new CsvReader(soilDbFileStream);

                while (!csvReader.EndOfStream())
                {
                    IDictionary<String, String> fields;
                    fields = csvReader.readLine();

                    if (fields == null || !fields.ContainsKey("dataObjName") || String.IsNullOrEmpty(fields["dataObjName"])) continue;
                    String name = fields["dataObjName"];

                    if (!soilData.ContainsKey(name))
                    {
                        soilData.Add(name, new Dictionary<Double, SoilValues>());
                    }
                    IDictionary<Double, SoilValues> soilValues = soilData[name];
                    SoilValues values = new SoilValues();
                    values.parseData(fields);
                    Double _iterator = Math.Round(Double.Parse(fields["_iterator.z"], CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero);
                    soilValues.Add(_iterator, values);
                    if (!maxDepth.ContainsKey(name))
                    {
                        maxDepth[name] = _iterator;
                    }
                    else
                    {
                        maxDepth[name] = Math.Max(maxDepth[name], _iterator);
                    }
                }
            }

            internal IDictionary<Double, SoilValues> getSoilData(String name)
            {
                return soilData[name];
            }

            internal double getMaxDepth(String name)
            {
                //return maxDepth[name] - DepthOffset;
                return maxDepth[name];
            }

            /*!
             * \brief   Get all soil names.
             *
             * \return  All soil names in database as unsorted list.
             */

            public ICollection<String> getSoilNames()
            {
                return soilData.Keys;
            }
        
            public Soil getSoil(String name)
            {
                //soil not in DB
                if (!soilData.ContainsKey(name)) return null;
                //soil instance already created
                if (soils.ContainsKey(name)) return soils[name];
                //create new instance
                soils[name] = new Soil(name, this);
                return soils[name];
            }
        }


        /*!
         * \brief   The local soil database. This class is instanciated only once at dll loading, the constructor is private.
         *
         * \remarks Access this class with static Method: atbApi.data.LocalSoilDb.Instance
         * \code{.unparsed}
         * 'get list with soilNames in database
         * Dim names = atbApi.data.LocalSoilDb.Instance.getSoilNames()
         * 'instantiate new soil with 3rd name in database
         * Dim soil As atbApi.data.Soil = New atbApi.data.Soil(names(3))
         * \endcode
         */

        public sealed class LocalSoilDb : SoilDb
        {
            private static String soilDataResource = "local.IWRM_ATB-SoilData.csv.gz";
            private static Stream soilDbGzipFileStream = ResourceStream.GetResourceStream(soilDataResource);
            private static Stream soilDbFileStream = new GZipStream(soilDbGzipFileStream, CompressionMode.Decompress);

            private static readonly LocalSoilDb instance = new LocalSoilDb(soilDbFileStream);

            private LocalSoilDb(Stream soilDbFileStream)
                : base(soilDbFileStream)
            {
                soilDbFileStream.Dispose();
                soilDbGzipFileStream.Dispose();
            }

            /*!
             * \brief   public property to get the single instance.
             *
             * \return  The instance of the dll builtin LocalSoilDb.
             */

            public static LocalSoilDb Instance
            {
                get
                {
                    return instance;
                }
            }
        }


        /*!
         * \brief   class to hold soil data and to provide useful functions for easy access to soil parameters
         *
         * \remarks Two constructors are available. If no soilDb is provided, the internal database is used.
         */
        public class Soil
        {
            private String _name;
            private double _maxDepth;
            private IDictionary<Double, SoilValues> soilData;

            /*!
             * \brief   public readonly property to access the name
             *
             * \return  The name of the soil from soilDb.
             */

            public String name { get { return this._name; } }

            /*!
             * \brief   public readonly property to access the maxDepth
             *
             * \return  The max Depth of the soil from soilDb.
             */

            public double maxDepth { get { return this._maxDepth; } }

            /*!
             * \brief   Constructor, creates a soil and read all data from the dll builtin soilDb
             *
             * \param   name    The soil name from the internel soil Db.
             * \code{.unparsed}
             * 'get list with soilNames from builtin soil database
             * Dim names = atbApi.data.LocalSoilDb.Instance.getSoilNames()
             * 'instantiate new soil with 7th name in names list
             * Dim internalSoil1 As atbApi.data.Soil = New atbApi.data.Soil(names(8))
             * 'instantiate new soil with hardcoded name
             * Dim internalSoil2 As atbApi.data.Soil = New atbApi.data.Soil("USDA-soilclass_sandy_clay_loam")
             * \endcode
             */

            public Soil(String name)
            {
                if (name == null) return;

                this._name = name;
                this._maxDepth = LocalSoilDb.Instance.getMaxDepth(_name);
                this.soilData = LocalSoilDb.Instance.getSoilData(_name);
            }

            /*!
             * \brief   Constructor, creates a soil and read all data from the provided soilDb
             *
             * \param   name    A soil name from the param soilDb.
             * \param   soilDb A soilDb instance.
             * \code{.unparsed}
             * 'create filestream for csv data file
             * Dim fs As System.IO.FileStream = New IO.FileStream("C:\DHI\MIKE-Basin_SoilData.csv", IO.FileMode.Open)
             * 'create new soilDb
             * Dim soilDb As atbApi.data.SoilDb = New atbApi.data.SoilDb(fs)
             * 'important: close fileStream after SoilDb is created
             * fs.Close()
             * 'create new soil and provide your own soilDb to constructor
             * Dim mySoil As atbApi.data.Soil = New atbApi.data.Soil("soilname_from_csv_data", soilDb)
             * \endcode
             */

            public Soil(String name, SoilDb soilDb)
            {
                if (name == null || soilDb == null) return;

                this._name = name;
                this._maxDepth = soilDb.getMaxDepth(_name);
                this.soilData = soilDb.getSoilData(_name);
            }

            /*!
             * \brief   Get values for a given depth.
             *
             * \param   z The depth of the soil query in meters, the step is 0.01 m.
             *
             * \return  The values if available for requested depth, else null is returned.
             */

            public SoilValues getValues(Double z)
            {
                if (soilData == null) return null;

                SoilValues resultSet;
                Double _z = Math.Round(z, 2, MidpointRounding.AwayFromZero);
                bool hasSet = soilData.TryGetValue(_z, out resultSet);
                if (hasSet) return resultSet;
                //try to get default set for average soils with only default set
                soilData.TryGetValue(0, out resultSet);
                return resultSet;
            }
        }
    }
}
