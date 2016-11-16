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

            private IList<String> requiredFields = { "seedDate", "harvestDate", "plant" };


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
                //use this for .net 4.0
                //foreach (PropertyInfo pi in values.GetType().GetRuntimeProperties()) {
                //use this for .net 4.5
                IList<String> piNames = new List<String>();
                foreach (PropertyInfo pi in values.GetType().GetRuntimeProperties()) {
                    piNames.Add(pi.Name);
                }

                foreach (String requiredField in requiredFields) {
                    if (piNames.Contains(requiredField)) continue;
                    return -1;
                }

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

            public IDictionary<String, ETResult> runCropSequence(DateTime start, DateTime end, TimeStep step, ref ETArgs etArgs)
            {
                foreach (CropSequenceValues cs in getCropSequence(start)) {

                }
                return results;
            }
        }
    }
}
