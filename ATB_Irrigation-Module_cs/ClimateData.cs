/*!
* \file		ClimateData.cs
*
* \brief	Class file for climate data types and access
*
* \author	Hunstock
* \date	09.07.2015
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.IO;
using System.Reflection;

using local;
using System.Net;

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

            public new void parseData(IDictionary<String, String> values)
            {
                base.parseData(values);
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
            private Exception _e;
            private TimeStep _step;
            private DateTime? _start;
            private DateTime? _end;
            private const String SpongeJsUrl = "https://agrohyd-api.runlevel3.de/ParameterSetData/getByTagTypeLocation/{0}/climate/{1}/{2}/date%3A{3}?end=date%3A{4}&step=date%3A{5}&format=csv";

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
             * \brief   public readonly property to access the last occured stream operations exception
             *
             * \return  The last exception or null if no exception occured.
             */

            public Exception lastException { get { return this._e; } }

            /*!
             * \brief   Constructor to create empty climate to be filled.
             *
             * \code{.unparsed}
             * \endcode
             */

            public Climate(TimeStep step)
            {
                _step = step;
            }

            private int loadCsv(Stream stream)
            {
                try
                {
                    CsvReader csvReader = new CsvReader(stream);

                    while (!csvReader.EndOfStream())
                    {
                        IDictionary<String, String> fields;
                        fields = csvReader.readLine();

                        if (fields == null) continue;
                        if (String.IsNullOrEmpty(fields["dataObjName"])) continue;
                        if (String.IsNullOrEmpty(_name)) _name = fields["dataObjName"];
                        //avoid to mix multiple stations, but for "nearest" it is nessecary
                        //if (!_name.Equals(fields["dataObjName"])) continue;

                        ClimateValues values = new ClimateValues();
                        values.parseData(fields);
                        DateTime _iterator = DateTime.Parse(fields["_iterator.date"], CultureInfo.InvariantCulture);
                        climateData.Add(Tools.AdjustTimeStep(_iterator, step), values);
                        _start = _start == null ? _iterator : new DateTime(Math.Min(start.Value.Ticks, _iterator.Ticks));
                        _end = _end == null ? _iterator : new DateTime(Math.Max(end.Value.Ticks, _iterator.Ticks));
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
             * \brief   Function to load external csv Stream and parse as climate data.
             *
             * \code{.unparsed}
             * \endcode
             */

            public int loadFromFileStream(Stream climateFileStream)
            {
                _e = null;
                return loadCsv(climateFileStream);
            }

            public async Task<int> loadFromATBWebService(Location location, DateTime start, DateTime end)
            {
                return await loadFromATBWebService(location, "public_service", "irri_mod_user", "irri_mod_pass", start, end);
            }

            /*!
             * \brief   Loads climate data per http request from the ATB/runlevel3 web service.
             *
             * \param   location    The location.
             * \param   tag         The tag.
             * \param   user        The user.
             * \param   pass        The pass.
             * \param   start       The start Date/Time.
             * \param   end         The end Date/Time.
             *
             * \return  The number of datasets in this climate, a number of 0 means that there was an exception and no data read.
             */

            public async Task<int> loadFromATBWebService(Location location, String tag, String user, String pass, DateTime start, DateTime end)
            {
                String url = String.Format(SpongeJsUrl,
                    tag,
                    location.lon.ToString(CultureInfo.InvariantCulture),
                    location.lat.ToString(CultureInfo.InvariantCulture),
                    start.ToUniversalTime().ToString("s") + "Z",
                    end.ToUniversalTime().ToString("s") + "Z",
                    "day"
                    );
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Credentials = new NetworkCredential(user, pass);
                try
                {
                    _e = null;
                    WebResponse res = await Task<WebResponse>.Factory.FromAsync(req.BeginGetResponse, req.EndGetResponse, req);
                    return loadCsv(res.GetResponseStream());
                }
                catch (Exception e)
                {
                    _e = e;
                    return 0;
                }
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
                climateData.Add(Tools.AdjustTimeStep(date, step), values);
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

            public ClimateValues getValues(DateTime date, TimeStep wantStep)
            {
                if (wantStep <= step)
                {
                    ClimateValues resultSet;
                    climateData.TryGetValue(Tools.AdjustTimeStep(date, step), out resultSet);
                    return resultSet;
                }
                else if (wantStep == TimeStep.day && step == TimeStep.hour)
                {

                }
                else if (wantStep == TimeStep.month && step == TimeStep.hour)
                {

                }
                else if (wantStep == TimeStep.month && step == TimeStep.day)
                {

                }
                return null;
            }
        }
    }
}
