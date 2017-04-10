/*!
 * \file	PlantData.cs
 *
 * \brief	Class file for plant database, parameter types and data access
 *  
 * \author	Hunstock
 * \date    09.07.2015
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
        * \brief	enum for the different plant stages.
        *
        * \remarks	standard are 4 stages, for winter crops, the development stage is divided into 3
        * pieces to better fit the development stagnation during frost.
        */

        public enum PlantStage
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
        public class PlantValues : BaseValues
        {
            /*! plant stage */
            public PlantStage? stage { get; set; }
            /*! crop coefficient */
            public Double? Kc { get; set; }
            /*! basal crop coefficient */
            public Double? Kcb { get; set; }
            /*! leaf area index */
            public Double? LAI { get; set; }
            /*! rooting depth without irrigation */
            public Double? Zr { get; set; }
            /*! rooting depth irrigated */
            public Double? Zr_irrigated { get; set; }
            /*! depletion fraction */
            public Double? p { get; set; }
            /*! yield response to water stress */
            public Double? Ky { get; set; }
            /*! yield response for all stages */
            public Double? Ky_total { get; set; }
            /*! plant height */
            public Double? height { get; set; }
            /*! true if this object is a fallow */
            public bool isFallow { get; set; }
            /*! botanical name  */
            public String botanicalName { get; set; }
            /*! german name */
            public String germanName { get; set; }
            /*! english name */
            public String englishName { get; set; }

            private static IDictionary<String, String> propertyMapper = new Dictionary<String, String>();

            static PlantValues()
            {
                // renamed property in Sponge-JS export tool
                // propertyMapper.Add("stage", "name");
            }

            public void parseData(IDictionary<String, String> values, CultureInfo cultureInfo = null)
            {
                base.parseData(values, propertyMapper, cultureInfo: cultureInfo);
            }
        };

        /*!
         * \brief   The plant database class.
         * 
         * \remarks This class and its public contructor is used to load an external csv-File as user defined plant database.
         *          Because of this portable dll cannot read files itself, a stream instance must be provided to read data from.
         *          The stream must be closed in the calling instance.
         *
         */

        public class PlantDb
        {
            private Stream plantDbFileStream;
            private IDictionary<String, Plant> plants = new Dictionary<String, Plant>();
            private ICollection<String> names = new HashSet<String>();
            private IDictionary<String, IDictionary<int, PlantValues>> plantData = new Dictionary<String, IDictionary<int, PlantValues>>();
            private IDictionary<String, int> plantStagesLength = new Dictionary<String, int>();
            private IDictionary<String, int> initialEnd = new Dictionary<String, int>();
            private IDictionary<String, int> developmentEnd = new Dictionary<String, int>();

            /*!
             * \brief   Constructor to load external csv-File as plant database.
             *
             * \param   plantDbFileStream   A file stream to read csv data from.
             *
             * \code{.unparsed}
             * 'create filestream for csv data file
             * Dim fs As System.IO.FileStream = New IO.FileStream("C:\DHI\MIKE-Basin_PlantData.csv", IO.FileMode.Open)
             * 'create new plantDb
             * Dim plantDb As atbApi.data.PlantDb = New atbApi.data.PlantDb(fs)
             * 'important: close fileStream after PlantDb is created
             * fs.Close()
             * 'create new plant and provide your own plantDb to constructor
             * Dim myPlant As atbApi.data.Plant = New atbApi.data.Plant("CROPWAT_80_Crop_data_CITRUS_70p_ca_bare", plantDb)
             * \endcode
             */

            public PlantDb(Stream plantDbFileStream, CultureInfo cultureInfo = null)
            {
                this.plantDbFileStream = plantDbFileStream;
                CsvReader csvReader = new CsvReader(plantDbFileStream);

                while (!csvReader.EndOfStream())
                {
                    IDictionary<String, String> fields;
                    fields = csvReader.readLine();

                    if (fields == null || !fields.ContainsKey("dataObjName") || String.IsNullOrEmpty(fields["dataObjName"])) continue;
                    String name = fields["dataObjName"];

                    if (!plantData.ContainsKey(name))
                    {
                        plantData.Add(name, new Dictionary<int, PlantValues>());
                        plantStagesLength.Add(name, 0);
                        initialEnd.Add(name, 0);
                    }
                    IDictionary<int, PlantValues> plantValues = plantData[name];
                    PlantValues values = new PlantValues();
                    values.parseData(fields, cultureInfo != null ? cultureInfo : CultureInfo.InvariantCulture);
                    int _iterator = Int32.Parse(fields["_iterator.day"], cultureInfo != null ? cultureInfo : CultureInfo.InvariantCulture);
                    plantValues.Add(_iterator, values);
                    plantStagesLength[name] = Math.Max(plantStagesLength[name], _iterator);
                    if (values.stage.Equals(PlantStage.initial))
                    {
                        if (!initialEnd.ContainsKey(name)) initialEnd[name] = 0;
                        initialEnd[name] = Math.Max(initialEnd[name], _iterator);
                    }
                    if (values.stage.Equals(PlantStage.development))
                    {
                        if (!developmentEnd.ContainsKey(name)) developmentEnd[name] = 0;
                        developmentEnd[name] = Math.Max(developmentEnd[name], _iterator);
                    }
                }
            }

            internal int getInitialEnd(String name)
            {
                if (initialEnd.ContainsKey(name)) return initialEnd[name];
                return 0;
            }

            internal int getDevelopmentEnd(String name)
            {
                if (developmentEnd.ContainsKey(name)) return developmentEnd[name];
                return 0;
            }

            internal int getStageTotal(String name)
            {
                if (plantStagesLength.ContainsKey(name)) return plantStagesLength[name];
                return 0;
            }

            internal IDictionary<int, PlantValues> getPlantData(String name)
            {
                if (plantData.ContainsKey(name)) return plantData[name];
                return null;
            }

            /*!
             * \brief   Get all plant names.
             *
             * \return  All plant names in database as unsorted list.
             */

            public ICollection<String> getPlantNames()
            {
                return plantData.Keys;
            }

            public Plant getPlant(String name)
            {
                //plant not in DB
                if (!plantData.ContainsKey(name)) return null;
                //plant instance already created
                if (plants.ContainsKey(name)) return plants[name];
                //create new instance
                plants[name] = new Plant(name, this);
                return plants[name];
            }
        }


        /*!
         * \brief   The local plant database. This class is instanciated only once at dll loading, the constructor is private.
         *
         * \remarks Access this class with static Method: atbApi.data.LocalPlantDb.Instance
         * \code{.unparsed}
         * 'get list with plantNames in database
         * Dim names = atbApi.data.LocalPlantDb.Instance.getPlantNames()
         * 'instantiate new plant with 5th name in database
         * Dim plant As atbApi.data.Plant = New atbApi.data.Plant(names(5))
         * \endcode
         */

        public sealed class LocalPlantDb : PlantDb
        {
            //plant data for public version
            //private static String plantDataResource = "local.IWRM_ATB-PlantData.csv.gz";
            //plant data for IWRM version
            private static String plantDataResource = "local.IWRM_ALL-PlantData.csv.gz";
            private static Stream plantDbGzipFileStream = ResourceStream.GetResourceStream(plantDataResource);
            private static Stream plantDbFileStream = new GZipStream(plantDbGzipFileStream, CompressionMode.Decompress);

            private static readonly LocalPlantDb instance = new LocalPlantDb(plantDbFileStream, plantDbGzipFileStream);

            private LocalPlantDb(Stream plantDbFileStream, Stream plantDbGzipFileStream)
                : base(plantDbFileStream)
            {
                plantDbFileStream.Dispose();
                plantDbGzipFileStream.Dispose();
            }

            /*!
             * \brief   public property to get the single instance.
             *
             * \return  The instance of the dll builtin LocalPlantDb.
             */

            public static LocalPlantDb Instance
            {
                get
                {
                    return instance;
                }
            }
        }


        /*!
         * \brief   class to hold plant data and to provide useful functions for easy access to plant parameters
         *
         * \remarks Two constructors are available. If no plantDb is provided, the internal database is used.
         */
        public class Plant
        {
            private String _name;
            private int _stageTotal;
            private int _initialEnd;
            private int _developmentEnd;
            private IDictionary<int, PlantValues> plantData;

            /*!
             * \brief   public readonly property to access the name
             *
             * \return  The name of the plant from plantDb.
             */

            public String name { get { return this._name; } }

            /*!
             * \brief   public readonly property to access initialLength.
             *
             * \return  The initialLength, the number of days in initial plant stage
             */

            public int initialEnd { get { return this._initialEnd; } }

            /*!
             * \brief   public readonly property to access developmentLength.
             *
             * \return  The idevelopmentLength, the number of days in development plant stage
             */

            public int developmentEnd { get { return this._developmentEnd; } }

            /*!
             * \brief   public readonly property to access stageTotal.
             *
             * \return  The stageTotal, the number of days from seeding to harvesting under standard conditions
             */

            public int stageTotal { get { return this._stageTotal; } }

            /*!
             * \brief   Constructor, creates a plant and read all data from the dll builtin plantDb
             *
             * \param   name    The plant name from the internel plantDb.
             * \code{.unparsed}
             * 'get list with plantNames from builtin plant database
             * Dim names = atbApi.data.LocalPlantDb.Instance.getPlantNames()
             * 'instantiate new plant with 24th name in names list
             * Dim internalPlant1 As atbApi.data.Plant = New atbApi.data.Plant(names(23))
             * 'instantiate new plant with hardcoded name
             * Dim internalPlant2 As atbApi.data.Plant = New atbApi.data.Plant("ATB_maize_grain_east_africa")
             * \endcode
             */

            public Plant(String name)
            {
                if (name == null) return;

                this._name = name;
                this.plantData = LocalPlantDb.Instance.getPlantData(_name);
                this._stageTotal = LocalPlantDb.Instance.getStageTotal(_name);
                this._initialEnd = LocalPlantDb.Instance.getInitialEnd(_name);
                this._developmentEnd = LocalPlantDb.Instance.getDevelopmentEnd(_name);
            }

            /*!
             * \brief   Constructor, creates a plant and read all data from the provided plantDb
             *
             * \param   name    A plant name from the param plantDb.
             * \param   plantDb A plantDb instance.
             * \code{.unparsed}
             * 'create filestream for csv data file
             * Dim fs As System.IO.FileStream = New IO.FileStream("C:\DHI\MIKE-Basin_PlantData.csv", IO.FileMode.Open)
             * 'create new plantDb
             * Dim plantDb As atbApi.data.PlantDb = New atbApi.data.PlantDb(fs)
             * 'important: close fileStream after PlantDb is created
             * fs.Close()
             * 'create new plant and provide yout own plantDb to constructor
             * Dim myPlant As atbApi.data.Plant = New atbApi.data.Plant("CROPWAT_80_Crop_data_CITRUS_70p_ca_bare", plantDb)
             * \endcode
             */

            public Plant(String name, PlantDb plantDb)
            {
                if (name == null || plantDb == null) return;

                this._name = name;
                this.plantData = plantDb.getPlantData(_name);
                this._stageTotal = plantDb.getStageTotal(_name);
                this._initialEnd = plantDb.getInitialEnd(_name);
                this._developmentEnd = plantDb.getDevelopmentEnd(_name);
            }

            internal Int32? getPlantDay(DateTime date, DateTime seedDate, DateTime harvestDate)
            {
                //return null if harvestDate is before or equal to seedDate or date not between seedDate and harvestDate
                if (DateTime.Compare(seedDate, harvestDate) >= 0) return null;
                if (DateTime.Compare(date, harvestDate) > 0) return null;
                if (DateTime.Compare(date, seedDate) < 0) return null;

                //build rounded dates to calculate with full days
                DateTime _date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                DateTime _seedDate = new DateTime(seedDate.Year, seedDate.Month, seedDate.Day, 0, 0, 0);
                DateTime _harvestDate = new DateTime(harvestDate.Year, harvestDate.Month, harvestDate.Day, 0, 0, 0);

                var interval = (_harvestDate - _seedDate).TotalDays;

                int day = (int)Math.Round((_date - _seedDate).TotalDays * (stageTotal / interval), MidpointRounding.AwayFromZero);
                if (day < 1) day = 1;
                if (day > stageTotal) day = stageTotal;

                return day;
            }

            /*!
             * \brief   Get values for a given day.
             *
             * \param   day The day of plant development. It must be >= 1 and <= stageTotal
             *
             * \return  The values if available for requested development day, else null is returned.
             */

            public PlantValues getValues(Int32? day)
            {
                if (plantData == null) return null;

                PlantValues resultSet;
                if (day != null)
                {
                    bool hasSet = plantData.TryGetValue((int)day, out resultSet);
                    if (hasSet) return resultSet;
                }
                //try to get default set for average plants with only default set
                plantData.TryGetValue(0, out resultSet);
                return resultSet;
            }

            /*!
             * \brief   Get values for a given day. Length of plant stages are proportional stretched or squeezed according
             *          to the real time between seedDate and harvestDate
             *
             * \param   date        The date of plant development. It must be between seedDate and harvestDate
             * \param   seedDate    The seed date.
             * \param   harvestDate The harvest date.
             *
             * \return  The values if available for requested development day, else null is returned.
             * \code{.unparsed}
             * Dim seedDate As Date = DateSerial(2004, 4, 13)
             * Dim harvestDate As Date = DateSerial(2004, 10, 5)
             * Dim actualDate As Date = DateSerial(2004, 5, 12)
             * Dim plantSet As atbApi.data.PlantValues = plant.getValues(actualDate, seedDate, harvestDate)
             * \endcode
             */

            public PlantValues getValues(DateTime date, DateTime seedDate, DateTime harvestDate)
            {
                return this.getValues(getPlantDay(date, seedDate, harvestDate));
            }

        }
    }
}
