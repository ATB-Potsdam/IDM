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
using System.IO;
using System.IO.Compression;
using local;

namespace atbApi
{
    namespace data
    {
        /*!
         * \brief   Soil values to track for continuous calculation without gap in the water balance.
         *
         */

        public struct SoilConditions
        {
            /*! drainage in the root zone */
            public double drRz;
            /*! totally available water in the root zone */
            public double tawRz;
            /*! drainage in the deep zone (below root zone up to 2m depth) */
            public double drDz;
            /*! totally available water in the deep zone (below root zone up to 2m depth) */
            public double tawDz;
            /*! drainage in the evaporation layer */
            public double de;
            /*! percolation from the evaporation layer */
            public double dpe;
            /*! rooting depth of the crop */
            public double zr;
        }

        /*!
         * \brief   Soil conditions for both calculation approaches: evapotranspiration and evaporation + transpiration
         *          This structure is used to calculate a crop sequence plant by plant continuously on one soil.
         *
         */

        public struct SoilConditionsDual
        {
            /*! conditions for evaporation + transpiration calculation */
            public SoilConditions e;
            /*! conditions for evapotranspiration calculation */
            public SoilConditions et;
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
            /*! humus content [%] */
            public double humus { get; set; }

            private static IDictionary<String, String> propertyMapper = new Dictionary<String, String>();

            static SoilValues()
            {
                // propertyMapper.Add("qfc", "fieldCapacity");
            }

            public new void parseData(IDictionary<String, String> values)
            {
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
            private Stream soilDbFileStream;
            private ICollection<String> names = new HashSet<String>();
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

                    String name = fields["dataObjName"];
                    if (String.IsNullOrEmpty(fields["dataObjName"])) continue;

                    if (!soilData.ContainsKey(name))
                    {
                        soilData.Add(name, new Dictionary<Double, SoilValues>());
                    }
                    IDictionary<Double, SoilValues> soilValues = soilData[name];
                    SoilValues values = new SoilValues();
                    values.parseData(fields);
                    Double _iterator = Math.Round(Double.Parse(fields["_iterator.z"], CultureInfo.InvariantCulture), 2);
                    soilValues.Add(_iterator, values);
                }
            }

            internal IDictionary<Double, SoilValues> getSoilData(String name)
            {
                return soilData[name];
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
            private IDictionary<Double, SoilValues> soilData;

            /*!
             * \brief   public readonly property to access the name
             *
             * \return  The name of the soil from soilDb.
             */

            public String name { get { return this._name; } }

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
                SoilValues resultSet;
                Double _z = Math.Round(z, 2);
                bool hasSet = soilData.TryGetValue(z, out resultSet);
                if (hasSet) return resultSet;
                //try to get default set for average soils with only default set
                soilData.TryGetValue(0, out resultSet);
                return resultSet;
            }
        }
    }
}
