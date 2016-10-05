/*!
* \file		CropSequenceData.cs
*
* \brief	Class file for cropSequence data types and access
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

using local;

namespace atbApi
{
    namespace data
    {

        /*!
        * \brief	defines a collection of climate values.
        *
        * \remarks	if "mean_temp" is ommitted, <c>max_temp + min_temp / 2</c> is used.
        */

        public class CropSequenceValues : BaseValues
        {
            public String networkId;
            public String fieldId;
            public DateTime seedDate;
            public DateTime harvestDate;
            public Plant plant;
            public Soil soil;
            public Climate climate;
            public IrrigationType irrigationType;

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

        public class CropSequence
        {
            /*! vector with data. */
            private IDictionary<DateTime, IDictionary<String, CropSequenceValues>> cropSequenceData = new Dictionary<DateTime, IDictionary<String, CropSequenceValues>>();
            private IDictionary<String, ETResult> _results = new Dictionary<String, ETResult>();

            private String _name;
            private String _dataObjId;
            private PlantDb _plantDb;
            private SoilDb _soilDb;
            private ClimateDb _climateDb;
            private Exception _e;
            private DateTime? _start;
            private DateTime? _end;

            /*!
             * \brief   public readonly property to access the name
             *
             * \return  The name of the climate station.
             */

            public String name { get { return this._name == null ? "uninitialized_cropSequence" : this._name; } }

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
             * \brief   public readonly property to access the plantDb
             *
             * \return  The plantDb in use.
             */
            public PlantDb plantDb { get { return this._plantDb; } }

            /*!
             * \brief   public readonly property to access the soilDb
             *
             * \return  The soilDb in use.
             */
            public SoilDb soilDb { get { return this._soilDb; } }

            /*!
             * \brief   public readonly property to access the climateDb
             *
             * \return  The climateDb in use.
             */
            public ClimateDb climateDb { get { return this._climateDb; } }

            public IDictionary<String, ETResult> results { get { return this._results; } }

            /*!
             * \brief   public readonly property to access the number of datasets in this instance
             *
             * \return  the number of datasets
             */
            public int count { get { return this.cropSequenceData.Count; } }

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

            public CropSequence(Stream cropSequenceFileStream, PlantDb plantDb, SoilDb soilDb, ClimateDb climateDb)
            {
                loadCsv(cropSequenceFileStream);
            }

            private String createNetworkFieldIndex(CropSequenceValues values)
            {
                if (String.IsNullOrEmpty(values.networkId) || String.IsNullOrEmpty(values.fieldId)) return null;
                return values.networkId + values.fieldId;
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

                        if (fields == null) continue;

                        CropSequenceValues values = new CropSequenceValues();
                        values.parseData(fields);
                        if (!fields.ContainsKey("_iterator.date")) continue;
                        DateTime _iterator = DateTime.Parse(fields["_iterator.date"], CultureInfo.InvariantCulture);
                        addValues(_iterator, values);
                    }
                    return cropSequenceData.Count;
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

            public int addValues(DateTime date, CropSequenceValues values)
            {
                DateTime _dateTimeIndex = Tools.AdjustTimeStep(date, TimeStep.day);
                if (!cropSequenceData.ContainsKey(_dateTimeIndex))
                {
                    cropSequenceData[_dateTimeIndex] = new Dictionary<String, CropSequenceValues>();
                }

                String networkFieldIndex = createNetworkFieldIndex(values);
                if (String.IsNullOrEmpty(networkFieldIndex)) return cropSequenceData.Count;

                cropSequenceData[_dateTimeIndex][networkFieldIndex] = values;

                _start = _start == null ? date : new DateTime(Math.Min(start.Value.Ticks, date.Ticks));
                _end = _end == null ? date : new DateTime(Math.Max(end.Value.Ticks, date.Ticks));

                return cropSequenceData.Count;
            }

            /*!
             * \brief   Get values for a given date.
             *
             * \param   date The date to get data for. It is adjusted to next lower bound of the timestep
             * \param   wantStep For automatic conversions of provided and needed data TimeSteps this argument is used.
             *
             * \return  The values if available for requested date, else null is returned.
             */

            public IDictionary<String, CropSequenceValues> getCropSequence(DateTime date)
            {
                if (cropSequenceData == null) return null;

                IDictionary<String, CropSequenceValues> resultSequence;
                cropSequenceData.TryGetValue(Tools.AdjustTimeStep(date, TimeStep.day), out resultSequence);
                return resultSequence;
            }

            public IDictionary<String, ETResult> runCropSequence(DateTime start, DateTime end, TimeStep step, ref ETArgs etArgs)
            {
                foreach (String networkFieldIndex in getCropSequence(start).Keys) {
                    results[networkFieldIndex] = new ETResult();
                }
                return results;
            }
        }
    }
}
