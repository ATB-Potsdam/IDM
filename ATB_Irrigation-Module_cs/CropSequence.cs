/*!
* \file		CropSequence.cs
*
* \brief	Class file for cropSequence data types, access and computation of a whole crop sequence for many fields.
*
* \author	Hunstock
* \date     09.07.2015
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
            /*! short name of the irrigation network, used to sum up the amount of irrigation demand per netword and field */
            public String networkId { get; set; }
            /*! field id in the irrigation network, used to sum up the amount of irrigation demand per netword and field */
            public String fieldId { get; set; }
            /*! seed date of the crop */
            public DateTime seedDate { get; set; }
            /*! harvest date of the crop */
            public DateTime harvestDate { get; set; }
            /*! plant object */
            public Plant plant { get; set; }
            /*! soil object */
            public Soil soil { get; set; }
            /*! climate object */
            public Climate climate { get; set; }
            /*! type of irrigation */
            public IrrigationType irrigationType { get; set; }
            /*! the field size */
            public double area { get; set; }
            /*! the rain pattern associated with the cropSequence to be used for distributing monthly rain sum to daily events */
            public RainPattern rainPattern { get; set; }
            /*! the water rigths, not internal used, relayed to caller to apply irrigation according to rights */
            public int waterRights { get; set; }
            /*! parameters to control autoIrrigation, see class documentation for details */
            public AutoIrrigationControl autoIrrigation { get; set; }
            /*! initial salinity of the root zone */
            public double salinityRzInitial { get; set; }
            /*! initial salinity of the root zone */
            public double salinityDzInitial { get; set; }
            /*! depletionRz The initial depletion in the root zone */
            public double depletionRzInitial { get; set; }
            /*! depletionDz The initial depletion in the zone under root up to 2m */
            public double depletionDzInitial { get; set; }
            /*! salinityECiw Electrical conductivity of irrigation water, this default value is used if no management option is given. */
            public double salinityECiw { get; set; }
            /*! ksSalinityMin Keep salinity low to not exceed this stress factor. Additional irrigation water will be applied according to "leachingStrategy". */
            public double ksSalinityMin { get; set; }
            /*! leachingStrategy If the water stress due to salinity is greater than "ksSalinityMin", extra water is added to decrease salinity. */
            public LeachingStrategy leachingStrategy { get; set; }

            private IDictionary<String, String> propertyMapper = new Dictionary<String, String>();

            /*!
             * \brief   Parse data, iterates through all properties with getter and setter and tries to set with
             * values.
             *
             * \param   values      The values.
             * \param   cultureInfo Information to parse data in different localized formats
             */

            public void parseData(IDictionary<String, String> values, CultureInfo cultureInfo = null, PlantDb pdb = null, SoilDb sdb = null, ClimateDb cdb = null, RainPatternDb rpdb = null)
            {
                base.parseData(values, propertyMapper, cultureInfo: cultureInfo, pdb: pdb, sdb: sdb, cdb: cdb, rpdb: rpdb);
                if (climate != null && rainPattern != null)
                {
                    climate.rainPattern = rainPattern;
                }
            }
        }

        /*!
         * \brief   Encapsulates the result of an irrigation calculation for one field only.
         *
         */

        public class CropSequenceFieldResult
        {
            /*! short name of the irrigation network, used to sum up the amount of irrigation demand per netword and field */
            public String networkId { get; set; }
            /*! field id in the irrigation network, used to sum up the amount of irrigation demand per netword and field */
            public String fieldId { get; set; }
            /*! the water rigths, not internal used, relayed to caller to apply irrigation according to rights */
            public int waterRights { get; set; }
            /*! area of the field to convert the irrigationDemand from unit mm to l. */
            public double area { get; set; }
            /*! calculated irrigation demand */
            public IrrigationAmount irrigationDemand { get; set; }
        }


        /*!
         * \brief   Encapsulates the result of an irrigation calculation.
         *          the results are grouped and sorted by irrigation districts and fields.
         *          This result from a dry run calculation can be modified and reinjected
         *          as irrigation allocation into a real calculation.
         *
         */

        public class CropSequenceResult
        {
            /*! runtimeMs, unit: "ms", description: Actual runtime of this model in milliseconds. */
            public double runtimeMs { get; set; }
            /*! error, unit: "none", description: If an error occured during the calculation, this value is not null and contains an error description. */
            public String error { get; set; }
            /*! irrigation demand of all districts and fields */
            public IDictionary<String, CropSequenceFieldResult> networkIdIrrigationDemand { get; set; }

            /*!
             * Default constructor.
             *
             */

            public CropSequenceResult()
            {
                networkIdIrrigationDemand = new Dictionary<String, CropSequenceFieldResult>();
            }
        }

        
        /*!
         * \brief   Class to hold crop sequence data for the calculation of crop growing for many fields at once.
         *          
         *
         */

        public class CropSequence
        {
            private IList<CropSequenceValues> cropSequenceData = new List<CropSequenceValues>();
            private IDictionary<String, ETResult> _results = new Dictionary<String, ETResult>();
            private IDictionary<String, Double> _areas = new Dictionary<String, Double>();
            private IList<String> requiredFields = new List<String>() { "seedDate", "harvestDate", "plant", "fieldId", "networkId" };

            private String _name;
            private String _dataObjId;
            private PlantDb _plantDb;
            private SoilDb _soilDb;
            private ClimateDb _climateDb;
            private RainPatternDb _rainPatternDb;
            private Exception _e;
            private DateTime? _start;
            private DateTime? _end;
            private CultureInfo _cultureInfo = null;

            /*!
             * \brief   public readonly property to access the name
             *
             * \return  The name of the climate station.
             */

            public String name { get { return this._name == null ? "uninitialized_cropSequence" : this._name; } }

            /*!
             * \brief   public readonly property to access the start date
             *
             * \return  The start date of all data available.
             */
            public DateTime? start { get { return this._start; } }

            /*!
             * \brief   public readonly property to access the end date
             *
             * \return  The start date of all data available.
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

            /*!
             * \brief   public readonly property to access the rainPatternDb
             *
             * \return  The rainPatternDb in use.
             */
            public RainPatternDb rainPatternDb { get { return this._rainPatternDb; } }

            /*!
             * \brief   public readonly property to access the detailed results
             *
             * \return  The results of all calculations
             */
            public IDictionary<String, ETResult> results { get { return this._results; } }

            /*!
             * \brief   public readonly property to access the area of each field to
             *          calculate total amount instead of mm, the same key like for results is used
             *
             * \return  The areas of all fields
             */
            public IDictionary<String, Double> areas { get { return this._areas; } }

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
             * \brief   Constructor to create the crop sequence and immediate load data from provided fileStream.
             *
             * \param   cropSequenceFileStream  File stream with csv data to load. It is automatically determined, if the data is gzipped.
             *                                  In this case the stream is internally unzipped.
             * \param   plantDb                 The plant database.
             * \param   soilDb                  The soil database.
             * \param   climateDb               The climate database.
             * \param   cultureInfo             Information to parse data in different localized formats
             */

            public CropSequence(Stream cropSequenceFileStream, PlantDb plantDb, SoilDb soilDb, ClimateDb climateDb, RainPatternDb rainPatternDb, CultureInfo cultureInfo = null)
            {
                _plantDb = plantDb;
                _climateDb = climateDb;
                _rainPatternDb = rainPatternDb;
                _soilDb = soilDb;
                _cultureInfo = cultureInfo;
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
                            values.parseData(fields, _cultureInfo == null ? CultureInfo.InvariantCulture : _cultureInfo, pdb: plantDb, sdb: soilDb, cdb: climateDb, rpdb: rainPatternDb);
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
             * \brief   Add values for a new sequence.
             *
             * \param   values  The new sequence values.
             *
             * \return  Number of all sequences.
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

                if (values.networkId != null && values.fieldId != null)
                {
                    String _csIndex = csIndex(values);
                    if (!_areas.ContainsKey(_csIndex)) _areas.Add(_csIndex, values.area);
                }

                return cropSequenceData.Count;
            }

            private String csIndex(CropSequenceValues csValues) {
                return csValues.networkId + '.' + csValues.waterRights + '.' + csValues.fieldId;
            }

            /*!
             * \brief   Get values for a given date.
             *
             * \param   date The date to get data for. It is adjusted to next lower bound of the timestep
             *
             * \return  List of sequence values available for requested date (seedDate <= date && harvestDate >= date), else empty List is returned.
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

            /*!
             * \brief   Add fallows to all cropSequence gaps for each networkId.waterRights.fieldId
             *
             * \param   the fallow plant
             * 
             * \return  number of added fallows
             */

            public int addFallows(Plant fallowCrop)
            {
                if (cropSequenceData == null) return 0;
                
                IDictionary<String, IDictionary<DateTime, CropSequenceValues>> csSorted = new Dictionary<String, IDictionary<DateTime, CropSequenceValues>>();
                int insertedFallows = 0;

                foreach (CropSequenceValues values in cropSequenceData)
                {
                    String csKey = csIndex(values);
                    if (!csSorted.ContainsKey(csKey))
                    {
                        csSorted[csKey] = new Dictionary<DateTime, CropSequenceValues>();
                    }
                    csSorted[csKey][values.seedDate] = values;
                }

                return insertedFallows;
            }

            //merge cropSequence args into given args
            private static ETArgs MergeArgs(ref ETArgs etArgs, CropSequenceValues cs)
            {
                ETArgs result = new ETArgs(etArgs);
                result.climate = cs.climate;
                result.plant = cs.plant;
                result.soil = cs.soil;
                result.seedDate = cs.seedDate;
                result.harvestDate = cs.harvestDate;
                if (cs.autoIrrigation != null) result.autoIrr = cs.autoIrrigation;
                return result;
            }

            /*!
             * Executes the crop sequence calculation.
             *
             * \author  Hunstock
             * \date    06.04.2017
             *
             * \param   start                       The start Date/Time for calculation.
             * \param   end                         The end Date/Time for calculation.
             * \param [in,out]  etArgs              The arguments for evaporation, transpiration and irrigation calculation.
             * \param [in,out]  irrigationAmount    The irrigation amounts. Here can be a modified CropSequenceResult passed.
             * \param   dryRun                      true to dry run and calculate the irrgation demand, false to real irrigate.
             *
             * \return  A CropSequenceResult, the irrigation demand, formatted for use in MIKE-Basin by DHI-WASY.
             */

            public CropSequenceResult runCropSequence(DateTime start, DateTime end, ref ETArgs etArgs, ref CropSequenceResult irrigationAmount, bool dryRun = false)
            {
                //measure calculation duration
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                CropSequenceResult localMbResult = new CropSequenceResult();
                if (irrigationAmount == null) irrigationAmount = new CropSequenceResult();

                IList<CropSequenceValues> csPart = getCropSequence(start);
                foreach (CropSequenceValues cs in csPart)
                {
                    String _csIndex = csIndex(cs);

                    ETArgs tmpArgs = MergeArgs(ref etArgs, cs);
                    tmpArgs.start = start;
                    tmpArgs.end = end;
                    if (tmpArgs.plant == null)
                    {
                        Debug.WriteLine("no plant for cropSequence: " + _csIndex + " skipping sequence " + start.ToString());
                        continue;
                    }

                    ETResult tmpResult = null;
                    if (_results.ContainsKey(_csIndex)) tmpResult = _results[_csIndex];

                    if (dryRun)
                    {
                        if (tmpResult != null) {
                            tmpResult = new ETResult(tmpResult);
                            tmpResult.autoIrrigation = 0;
                            tmpResult.autoNetIrrigation = 0;
                            tmpResult.interceptionAutoIrr = 0;
                        }
                        Transpiration.ETCalc(ref tmpArgs, ref tmpResult, dryRun);
                    }
                    else
                    {
                        if (irrigationAmount.networkIdIrrigationDemand.ContainsKey(_csIndex))
                        {
                            //Debug.WriteLine("found irrigation: " + csIndex + ": " + mbResult.networkIdIrrigationDemand[csIndex].ToString());
                            //FIXME: convert irrigation to schedule and add to tmpArgs
                            DateTime startMin = new DateTime(Math.Max(tmpArgs.seedDate.Ticks, tmpArgs.start != null ? ((DateTime)tmpArgs.start).Ticks : 0));
                            DateTime endMax = new DateTime(Math.Min(tmpArgs.harvestDate.Ticks, tmpArgs.end != null ? ((DateTime)tmpArgs.end).Ticks : 0));
                            TimeSpan scheduleLength = endMax.Subtract(startMin);
                            DateTime scheduleMid = startMin.AddMinutes(scheduleLength.TotalMinutes / 2);
                            if (tmpArgs.irrigationSchedule == null)
                            {
                                if (cs.irrigationType == null)
                                {
                                    tmpArgs.irrigationSchedule = new IrrigationSchedule(cs.autoIrrigation.type);
                                    tmpArgs.autoIrr.type = cs.autoIrrigation.type;
                                }
                                else
                                {
                                    tmpArgs.irrigationSchedule = new IrrigationSchedule(cs.irrigationType);
                                }
                                tmpArgs.irrigationSchedule.schedule.Add(scheduleMid, irrigationAmount.networkIdIrrigationDemand[_csIndex].irrigationDemand.amount);
                            }
                        }
                        Transpiration.ETCalc(ref tmpArgs, ref tmpResult, dryRun);
                        _results[_csIndex] = tmpResult;
                    }

                    if (localMbResult.networkIdIrrigationDemand.ContainsKey(_csIndex))
                    {
                        localMbResult.networkIdIrrigationDemand[_csIndex].irrigationDemand.surfaceWater.amount += tmpResult.autoNetIrrigation;
                    }
                    else
                    {
                        localMbResult.networkIdIrrigationDemand[_csIndex] = new CropSequenceFieldResult();
                        localMbResult.networkIdIrrigationDemand[_csIndex].area = cs.area;
                        localMbResult.networkIdIrrigationDemand[_csIndex].fieldId = cs.fieldId;
                        localMbResult.networkIdIrrigationDemand[_csIndex].networkId = cs.networkId;
                        localMbResult.networkIdIrrigationDemand[_csIndex].waterRights = cs.waterRights;
                        localMbResult.networkIdIrrigationDemand[_csIndex].irrigationDemand = new IrrigationAmount(surfaceWaterAmount: tmpResult.autoNetIrrigation);
                    }

                    localMbResult.error += tmpResult.error;
                }
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                String elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds);
                Debug.WriteLine("runCropSequence took " + elapsedTime + " start: " + start.ToString("yyyy-MM-dd") + " end: " + end.ToString("yyyy-MM-dd"));
                localMbResult.runtimeMs = stopWatch.ElapsedMilliseconds;

                return localMbResult;
            }
        }
    }
}
