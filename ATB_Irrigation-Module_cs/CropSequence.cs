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
using System.Reflection;

using local;

namespace atbApi
{
    namespace data
    {

        /*!
        * \brief	defines a collection of cropSequence values.
        *
        * \remarks	if "mean_temp" is ommitted, <c>max_temp + min_temp / 2</c> is used.
        */

        public class CropSequenceValues : BaseValues
        {
            public String networkId { get; set; }
            public String fieldId { get; set; }
            public DateTime seedDate { get; set; }
            public DateTime harvestDate { get; set; }
            public Plant plant { get; set; }
            public Soil soil { get; set; }
            public Climate climate { get; set; }
            public IrrigationType irrigationType { get; set; }

            private static IDictionary<String, String> propertyMapper = new Dictionary<String, String>();

            public void parseData(IDictionary<String, String> values, PlantDb pdb = null, SoilDb sdb = null, ClimateDb cdb = null)
            {
                base.parseData(values, propertyMapper, pdb: pdb, sdb: sdb, cdb: cdb);
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
            //private IDictionary<DateTime, IDictionary<String, CropSequenceValues>> cropSequenceData = new Dictionary<DateTime, IDictionary<String, CropSequenceValues>>();
            private IList<CropSequenceValues> cropSequenceData = new List<CropSequenceValues>();
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

            private IList<String> requiredFields = new List<String>(){ "seedDate", "harvestDate", "plant" };


            /*!
             * \brief   Constructor to create climate and immediate load data from provided fileStream.
             *
             * \param   climateFileStream   File stream with csv data to load. The name, location and altitude are taken from this data.
             * \param   step                incremental time step of the data
             */

            public CropSequence(Stream cropSequenceFileStream, PlantDb plantDb, SoilDb soilDb, ClimateDb climateDb)
            {
                _plantDb = plantDb;
                _climateDb = climateDb;
                _soilDb = soilDb;
                loadCsv(cropSequenceFileStream);
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

                        CropSequenceValues values = new CropSequenceValues();
                        //catch parse exception and continue reading file
                        try
                        {
                            values.parseData(fields, pdb: plantDb, sdb: soilDb, cdb: climateDb);
                        }
                        catch (Exception e)
                        {
                            _e = e;
                            continue;
                        }
                        
                        bool fieldMissing = false;
                        foreach (String requiredField in requiredFields) {
                            if (!fields.ContainsKey(requiredField))
                            {
                                fieldMissing = true;
                                break;
                            }
                        }
                        if (fieldMissing) continue;

                        addValues(values);
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

            public int addValues(CropSequenceValues values)
            {
                IList<String> piNames = new List<String>();
                //use this for .net 4.0
                //foreach (PropertyInfo pi in values.GetType().GetRuntimeProperties()) {
                //use this for .net 4.5
                foreach (PropertyInfo pi in values.GetType().GetRuntimeProperties()) {
                    piNames.Add(pi.Name);
                }

                foreach (String requiredField in requiredFields) {
                    if (piNames.Contains(requiredField)) continue;
                    return -1;
                }

                //FIXME: set _start and _end
                if (!_start.HasValue || values.seedDate < _start) _start = values.seedDate;
                if (!_end.HasValue || values.harvestDate > _end) _end = values.harvestDate;

                cropSequenceData.Add(values);

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

            public IList<CropSequenceValues> getCropSequence(DateTime date)
            {
                if (cropSequenceData == null) return null;

                IList<CropSequenceValues> resultSequence = new List<CropSequenceValues>();
                foreach (CropSequenceValues values in cropSequenceData) {
                    if (values.seedDate <= date && values.harvestDate >= date) resultSequence.Add(values);
                }
                return resultSequence;
            }

            private static ETArgs MergeArgs(ref ETArgs etArgs, CropSequenceValues cs)
            {
                ETArgs result = new ETArgs(etArgs);
                result.climate = cs.climate;
                result.plant = cs.plant;
                result.soil = cs.soil;
                result.seedDate = cs.seedDate;
                result.harvestDate = cs.harvestDate;
                return result;
            }

            public IDictionary<String, ETResult> runCropSequence(DateTime start, DateTime end, TimeStep step, ref ETArgs etArgs)
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                foreach (CropSequenceValues cs in getCropSequence(start))
                {
                    String csIndex = cs.networkId + cs.fieldId;
                    ETResult tmpResult = null;
                    ETArgs tmpArgs = MergeArgs(ref etArgs, cs);
                    tmpArgs.start = start;
                    tmpArgs.end = end;
                    if (_results.ContainsKey(csIndex))
                    {
                        tmpResult = _results[csIndex];
                        tmpArgs.lastConditions = tmpResult.lastConditions;
                        tmpResult.lastConditions = null;
                    }
                    Transpiration.ETCalc(ref tmpArgs, ref tmpResult);
                    _results[csIndex] = tmpResult;
                }
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds);
                Debug.WriteLine("runCropSequence took " + elapsedTime); 
                return results;
            }
        }
    }
}
