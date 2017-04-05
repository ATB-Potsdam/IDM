/*!
* \file		ClimateData.cs
*
* \brief	Class file for climate data types and access
*
* \author	Hunstock
* \date	09.07.2015
*/

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
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
            /*! et0 [mm] precalculated value. */
            public Double? et0 { get; set; }
            /*! cloudiness [1/8] optional value. */
            public Double? cloudiness { get; set; }
            /*! air pressure [hPa] optional value. */
            public Double? air_pressure { get; set; }
            /*! vapour pressure [hPa] optional value. */
            public Double? vapour_pressure { get; set; }

            private static IDictionary<String, String> propertyMapper = new Dictionary<String, String>();

            public ClimateValues()
            {
                // renamed property in Sponge-JS export tool
                // propertyMapper.Add("xxx", "name");
            }

            public new void parseData(IDictionary<String, String> values, CultureInfo cultureInfo = null)
            {
                base.parseData(values, propertyMapper, cultureInfo: cultureInfo);
            }

            //clone constructor
            public ClimateValues(ClimateValues climateValues)
            {
                //use this for .net 4.0
                //foreach (PropertyInfo pi in this.GetType().GetProperties())
                //use this for .net 4.5
                foreach (PropertyInfo pi in this.GetType().GetRuntimeProperties())
                {
                    if (climateValues.GetType().GetRuntimeProperty(pi.Name) == null) continue;
                    pi.SetValue(this, climateValues.GetType().GetRuntimeProperty(pi.Name).GetValue(climateValues, null), null);
                }
            }
        }

        /*!
         * \brief   climate database static class ist instantiated at library loading
         *          contains all climate names/_ids and data
         *          
         *          This class is bound to a Sponge-JS web service from where the climate _ids and
         *          names are loaded at initialisation.
         *
         */

        public static class ClimateWebServiceDb
        {
            private static IDictionary<String, String> climateIds = new Dictionary<String, String>();
            private static IDictionary<String, Climate> climateInstances = new Dictionary<String, Climate>();

            private static async Task<int> initializeInstance(String tag)
            {
                climateIds.Clear();
                climateInstances.Clear();
                CsvReader csvReader = new CsvReader(await WebApiRequest.LoadClimateIdsFromATBWebService(tag));

                while (!csvReader.EndOfStream())
                {
                    IDictionary<String, String> fields;
                    fields = csvReader.readLine();

                    if (String.IsNullOrEmpty(fields["name"]) || String.IsNullOrEmpty(fields["_id"])) continue;
                    climateIds.Add(fields["name"], fields["_id"]);
                }
                return climateIds.Count;
            }

            /*!
             * \brief   Initialize this instance, if not initialized and loads
             *          all climate names from the web service.
             *
             * \param   reinit  true to reinitialise.
             * \param   tag     set to prefer a different tag, can be null for default tag
             *
             * \return  The list of climate names.
             */

            public static async Task<ICollection<String>> getClimateNames(bool reinit, String tag)
            {
                if (climateIds.Count == 0 || reinit == true) await initializeInstance(tag);
                return climateIds.Keys;
            }

            /*!
             * \brief   Gets a climate.
             *
             * \param   name    The name.
             * \param   start   The start Date/Time.
             * \param   end     The end Date/Time.
             * \param   step    requested time step
             *
             * \return  The climate.
             */

            public static async Task<Climate> getClimate(String name, DateTime start, DateTime end, TimeStep step)
            {
                Climate _climate = climateInstances.ContainsKey(name) ? climateInstances[name] : new Climate(climateIds[name], name, step);
                climateInstances[name] = _climate;

                await _climate.loadClimateByIdFromATBWebService(start, end);
                return _climate;
            }
        }

        public class ClimateDb
        {
            private IDictionary<String, Climate> climates = new Dictionary<String, Climate>();

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

            public ClimateDb()
            {
            }

            internal Climate getClimate(String name)
            {
                return climates.ContainsKey(name) ? climates[name] : null;
            }

            public void addClimate(Stream climateFileStream, TimeStep step, CultureInfo cultureInfo = null)
            {
                Climate _climate = new Climate(climateFileStream, step, cultureInfo);
                climates[_climate.name] = _climate;
            }

            /*!
             * \brief   Get all soil names.
             *
             * \return  All soil names in database as unsorted list.
             */

            public ICollection<String> getClimateNames()
            {
                return climates.Keys;
            }
        }


        /*!
         * \brief   Class to hold aggregated climate data for the calculation of a crop growing: The data must begin
         *          at seed date and must not end before harvest date. 
         *
         */

        public class Climate
        {
            /*! vector with data. */
            private IDictionary<DateTime, ClimateValues> climateData = new Dictionary<DateTime, ClimateValues>();
            private String _name;
            private String _dataObjId;
            private Location _location;
            private Exception _e;
            private TimeStep _step;
            private DateTime? _start;
            private DateTime? _end;
            private CultureInfo _cultureInfo = null;

            /*!
             * \brief   public readonly property to access the name
             *
             * \return  The name of the climate station.
             */

            public String name { get { return this._name == null ? "uninitialized_climate" : this._name; } }

            /*!
             * \brief   public readonly property to access the name
             *
             * \return  The incremental step of data vector.
             */
            public TimeStep step { get { return this._step; } }

            /*!
             * \brief   public readonly property to access the start date
             *
             * \return  The start date of data vector.
             */
            public DateTime? start { get { return this._start; } }

            /*!
             * \brief   public readonly property to access the end date
             *
             * \return  The start date of data vector.
             */
            public DateTime? end { get { return this._end; } }

            /*!
             * \brief   public readonly property to access the location
             *
             * \return  The location object of climate station.
             */
            public Location location { get { return this._location; } }

            /*!
             * \brief   public readonly property to access the number of datasets in this instance
             *
             * \return  the number of datasets
             */
            public int count { get { return this.climateData.Count; } }

            /*!
             * \brief   public readonly property to access the last occured stream operations exception
             *
             * \return  The last exception or null if no exception occured.
             */

            public Exception lastException { get { return this._e; } }

            /*!
             * \brief   Constructor to create climate and immediate load data from provided fileStream.
             *
             * \param   climateFileStream   File stream with csv data to load. The name, location and altitude are taken from this data.
             * \param   step                incremental time step of the data
             */

            public Climate(Stream climateFileStream, TimeStep step, CultureInfo cultureInfo = null)
            {
                _step = step;
                _cultureInfo = cultureInfo;
                loadCsv(climateFileStream);
            }

            /*!
             * \brief   Constructor to create empty climate to be filled.
             *
             * \param   name    The name of the climate.
             * \param   step    incremental time step of the data
             */

            public Climate(String name, TimeStep step, CultureInfo cultureInfo = null)
            {
                _step = step;
                _name = name;
                _cultureInfo = cultureInfo;
            }

            /*!
             * \brief   Constructor to create empty climate to be filled.
             * 
             * \param   name        The name of the climate.
             * \param   dataObjId   Database _id of the data object. With this _id the needed data is requested from the ATB web service
             * \param   step        incremental time step of the data
             */

            public Climate(String name, String dataObjId, TimeStep step, CultureInfo cultureInfo = null)
            {
                _step = step;
                _name = name;
                _dataObjId = dataObjId;
                _cultureInfo = cultureInfo;
            }

            private ClimateValues convertTimeStep(ClimateValues climateSet, DateTime adjustedDate, TimeStep nativeStep, TimeStep requestedStep)
            {
                if (!(nativeStep == TimeStep.month && requestedStep == TimeStep.day))
                    throw new NotImplementedException();
                int daysInMonth = DateTime.DaysInMonth(adjustedDate.Year, adjustedDate.Month);
                var resultSet = new ClimateValues(climateSet);
                resultSet.precipitation /= daysInMonth;
                resultSet.sunshine_duration /= daysInMonth;
                resultSet.Rs /= daysInMonth;
                resultSet.et0 /= daysInMonth;
                return resultSet;
            }

            private int loadCsv(Stream stream)
            {
                try
                {
                    _e = null;
                    CsvReader csvReader = new CsvReader(stream);

                    while (!csvReader.EndOfStream())
                    {
                        IDictionary<String, String> fields;
                        fields = csvReader.readLine();

                        if (fields == null || !fields.ContainsKey("dataObjName") || String.IsNullOrEmpty(fields["dataObjName"])) continue;
                        if (String.IsNullOrEmpty(_name)) _name = fields["dataObjName"];

                        if (fields.ContainsKey("dataObjId") && String.IsNullOrEmpty(_dataObjId) && !String.IsNullOrEmpty(fields["dataObjId"]))
                        {
                            _dataObjId = fields["dataObjId"];
                        }
                        //avoid to mix multiple stations, but for "nearest" it is nessecary
                        //if (!_name.Equals(fields["dataObjName"])) continue;

                        ClimateValues values = new ClimateValues();
                        values.parseData(fields, _cultureInfo == null ? CultureInfo.InvariantCulture : _cultureInfo);

                        if (!fields.ContainsKey("_iterator.date")) continue;
                        DateTime _iterator = DateTime.Parse(fields["_iterator.date"], _cultureInfo == null ? CultureInfo.InvariantCulture : _cultureInfo);
                        if (_step != TimeStep.day)
                        {
                            addValues(_iterator, convertTimeStep(values, Tools.AdjustTimeStep(_iterator, this.step), this.step, TimeStep.day));
                        }
                        else
                        {
                            addValues(_iterator, values);
                        }
                    }
                    return climateData.Count;
                }
                catch (Exception e)
                {
                    _e = e;
                    return 0;
                }
            }

            /*!
             * \brief   Loads climate data per http request from the ATB/runlevel3 web service.
             *
             * \param   location    The location to get nearest station data from.
             * \param   tag         The tag to prefer climate data with this tag, can be null to use default tag.
             * \param   start       The start Date/Time of requested data.
             * \param   end         The end Date/Time of requested data.
             *
             * \return  The number of datasets in this climate, a number of 0 means that there was an exception and no data read.
             */

            public async Task<int> loadClimateByLocationTagFromATBWebService(Location location, String tag, DateTime start, DateTime end)
            {
                _location = location;
                if (_start == null || _start > start || _end == null || _end < end)
                {
                    return loadCsv(await WebApiRequest.LoadClimateByLocationTagFromATBWebService(_location, tag, start, end, this._step));
                }
                else return climateData.Count;
            }

            /*!
             * \brief   Loads climate data per http request from the ATB/runlevel3 web service.
             *          The dataObjId has to be provided in advance with the contructor to use this method.
             *
             * \param   start       The start Date/Time of requested data.
             * \param   end         The end Date/Time of requested data.
             *
             * \return  The number of datasets in this climate, a number of 0 means that there was an exception and no data read.
             */

            public async Task<int> loadClimateByIdFromATBWebService(DateTime start, DateTime end)
            {
                if (String.IsNullOrEmpty(_dataObjId)) return 0;

                var baseData = await WebApiRequest.LoadBaseDataByIdFromATBWebService(_dataObjId);
                if (baseData["location"] != null)
                {
                    _location.lon = (double)baseData["location"][0];
                    _location.lat = (double)baseData["location"][1];
                    if (baseData["altitude"] != null)
                    {
                        _location.alt = (double)baseData["altitude"];
                    }
                }
                if (_start == null || _start > start || _end == null || _end < end)
                {
                    return loadCsv(await WebApiRequest.LoadClimateByIdFromATBWebService(_dataObjId, start, end, this._step));
                }
                else return climateData.Count;
            }

            /*!
             * \brief   Loads climate station altitude per http request from the ATB/runlevel3 web service.
             *
             * \param   location    The location to get altitude for.
             *
             * \return  The altitude at this location.
             */

            public async Task<double> loadAltitudeFromATBWebService(Location location)
            {
                if (_location.alt.HasValue) return (double)_location.alt;

                _location = location;
                _location.alt = await WebApiRequest.LoadAltitudeFromATBWebService(location);
                return (double)_location.alt;
            }


            /*!
             * \brief   Add values for a given date.
             *
             * \param   date The date to set data for. It is adjusted to next lower bound of the timestep
             * \param   values  The climate values.
             * \result  number of datasets
             */

            public int addValues(DateTime date, ClimateValues values)
            {
                climateData[Tools.AdjustTimeStep(date, step)] = values;
                _start = _start == null ? date : new DateTime(Math.Min(start.Value.Ticks, date.Ticks));
                _end = _end == null ? date : new DateTime(Math.Max(end.Value.Ticks, date.Ticks));
                return climateData.Count;
            }

            /*!
             * \brief   Get values for a given date.
             *
             * \param   date The date to get data for. It is adjusted to next lower bound of the timestep
             * \param   wantStep For automatic conversions of provided and needed data TimeSteps this argument is used.
             *
             * \return  The values if available for requested date, else null is returned.
             */

            public ClimateValues getValues(DateTime date)
            {
                if (climateData == null) return null;

                ClimateValues resultSet;
                DateTime adjustedDate = Tools.AdjustTimeStep(date, this.step);
                climateData.TryGetValue(adjustedDate, out resultSet);

                return resultSet;
            }
        }
    }
}
