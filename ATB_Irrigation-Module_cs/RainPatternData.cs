/*!
* \file		RainPatternData.cs
*
* \brief	Class file for rainPattern data types and access
*
* \author	Hunstock
* \date     02.08.2017
*/

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;

using local;
using atbApi.tools;

namespace atbApi
{
    namespace data
    {

        /*!
        * \brief	defines a collection of rainPattern values.
        *
        */

        public class RainPatternValues : BaseValues
        {
            /*! The month */
            public int month { get; set; }
            /*! Monthly precipitation sum [mm] */
            public Double sum { get; set; }
            /*! year when rainpattern occured */
            public int year { get; set; }
            /*! real pattern */
            public Double[] pattern { get; set; }

            private IDictionary<String, String> propertyMapper = new Dictionary<String, String>();

            /*!
             * \brief   Default constructor.
             *
             */

            public RainPatternValues()
            {
                 propertyMapper.Add("_id", "dataObjName");
            }

            /*!
             * \brief   Parse data, iterates through all properties with getter and setter and tries to set with
             * values.
             *
             * \param   values      The values.
             * \param   cultureInfo Information to parse data in different localized formats
             */

            public void parseData(IDictionary<String, String> values, CultureInfo cultureInfo = null)
            {
                base.parseData(values, propertyMapper, cultureInfo: cultureInfo);
            }

            /*!
             * \brief   Clone Constructor.
             *
             * \param   rainPatternValues   The original rainPattern values to clone.
             */

            public RainPatternValues(RainPatternValues rainPatternValues)
            {
                //use this for .net 4.0
                //foreach (PropertyInfo pi in this.GetType().GetProperties())
                //use this for .net 4.5
                foreach (PropertyInfo pi in this.GetType().GetRuntimeProperties())
                {
                    if (rainPatternValues.GetType().GetRuntimeProperty(pi.Name) == null) continue;
                    pi.SetValue(this, rainPatternValues.GetType().GetRuntimeProperty(pi.Name).GetValue(rainPatternValues, null), null);
                }
            }
        }

        /*!
         * \brief   A rainPattern database.
         *          This class holds several rainPattern objects to be used in conjunction with a cropSequence.
         *          Each rainPattern is stored and accessed by name in the database.
         *
         */

        public class RainPatternDb
        {
            private IDictionary<String, RainPattern> rainPatterns = new Dictionary<String, RainPattern>();

            /*!
             * \brief   Default constructor.
             *
             */

            public RainPatternDb()
            {
            }

            /*!
             * \brief   Gets a rainPattern from the database.
             *
             * \param   name    The name of the rainPattern in the database.
             *
             * \return  The rainPattern class with all available data.
             */

            internal RainPattern getRainPattern(String name)
            {
                return rainPatterns.ContainsKey(name) ? rainPatterns[name] : null;
            }

            /*!
             * \brief   Function to load external csv-File as new rainPattern into the database.
             *
             * \param   rainPatternFileStream   A file stream to read csv data from. It is automatically determined, if the data is gzipped.
             *                              In this case the stream is internally unzipped.
             * \param   step                Data in csv-file is organized in this timestep.
             * \param   cultureInfo         Information describing the culture.
             *
             * \remarks The following code example reads 6 different csv files into the database.
             *
             * \return  The number if rainPatterns in this database.
             */

            public int addRainPattern(Stream rainPatternFileStream, CultureInfo cultureInfo = null)
            {
                RainPattern _rainPattern = new RainPattern(rainPatternFileStream, cultureInfo: cultureInfo);
                rainPatterns[_rainPattern.name] = _rainPattern;
                return rainPatterns.Count;
            }

            /*!
             * \brief   Get all rainPattern names.
             *
             * \return  All rainPattern names in database as unsorted list.
             */

            public ICollection<String> getRainPatternNames()
            {
                return rainPatterns.Keys;
            }
        }


        /*!
         * \brief   Class to hold aggregated rainPattern data for the calculation of a crop growing: The data must begin
         *          at seed date and must not end before harvest date. 
         *
         */

        public class RainPattern
        {
            /*! vector with data. */
            private IDictionary<int, SortedDictionary<Double, RainPatternValues>> rainPatternData = new Dictionary<int, SortedDictionary<Double, RainPatternValues>>();
            private Dictionary<int, List<Double>> keys = new Dictionary<int, List<Double>>();
            private String _name;
            private String _dataObjId;
            private Exception _e;
            private CultureInfo _cultureInfo = null;

            /*!
             * \brief   public readonly property to access the name
             *
             * \return  The name of the rainPattern station.
             */

            public String name { get { return this._name == null ? "uninitialized_rainPattern" : this._name; } }

            /*!
             * \brief   public readonly property to access the number of datasets in this instance
             *
             * \return  the number of datasets
             */
            public int count { get { return this.rainPatternData.Count; } }

            /*!
             * \brief   public readonly property to access the last occured stream operations exception
             *
             * \return  The last exception or null if no exception occured.
             */

            public Exception lastException { get { return this._e; } }

            /*!
             * \brief   Constructor to create rainPattern and immediate load data from provided fileStream.
             *
             * \param   rainPatternFileStream   File stream with csv data to load. The name, location and altitude are taken from this data.
             */

            public RainPattern(Stream rainPatternFileStream, CultureInfo cultureInfo = null)
            {
                _cultureInfo = cultureInfo;
                loadCsv(rainPatternFileStream);
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

                        if (fields == null || (
                            (!fields.ContainsKey("dataObjName") || String.IsNullOrEmpty(fields["dataObjName"]))
                            && (!fields.ContainsKey("_id") || String.IsNullOrEmpty(fields["_id"]))
                            )) continue;
                        if (String.IsNullOrEmpty(_name)) _name = fields.ContainsKey("dataObjName") ? fields["dataObjName"] : fields["_id"];

                        if (fields.ContainsKey("dataObjId") && String.IsNullOrEmpty(_dataObjId) && !String.IsNullOrEmpty(fields["dataObjId"]))
                        {
                            _dataObjId = fields["dataObjId"];
                        }

                        RainPatternValues values = new RainPatternValues();
                        values.parseData(fields, _cultureInfo == null ? CultureInfo.InvariantCulture : _cultureInfo);

                        //month and sum are required
                        if (!fields.ContainsKey("month") || !fields.ContainsKey("sum")) continue;

                        addValues(values.month, values.sum, values);
                    }
                    return rainPatternData.Count;
                }
                catch (Exception e)
                {
                    _e = e;
                    return 0;
                }
            }

            /*!
             * \brief   Add values for a given month and sum.
             *
             * \param   month   The when rainPattern occurs
             * \param   sum     The sum of this particular month sample
             * \param   values  The rainPattern values.
             *
             * \return  number of datasets.
             */

            public int addValues(int month, Double sum, RainPatternValues values)
            {
                if (!rainPatternData.ContainsKey(month))
                {
                    rainPatternData[month] = new SortedDictionary<double, RainPatternValues>();
                }
                rainPatternData[month][sum] = values;
                keys[month] = new List<Double>(rainPatternData[month].Keys);
                return rainPatternData.Count;
            }


            /*!
             * \brief   Get value for a given date and find pattern for best matching sum
             *
             * \param   date The date to get data for. Only month and day of month are extracted
             * \param   sum needed to find best match of monthly rainPattern sum.
             *
             * \return  The values if available for requested date, else null is returned.
             */

            public Double? getValue(DateTime date, Double sum)
            {
                if (rainPatternData == null) return null;

                SortedDictionary<Double, RainPatternValues> rainPatternList;
                rainPatternData.TryGetValue(date.Month, out rainPatternList);
                if (rainPatternList == null) return null;
                //returns 0..n for exact match
                //< 0 is the negative index of last smaller
                int bestMatchIndex = keys[date.Month].BinarySearch(sum);
                //exact match
                if (bestMatchIndex >= 0)
                {
                    return rainPatternList[keys[date.Month][bestMatchIndex]].pattern[date.Day - 1];
                }
                else
                {
                    RainPatternValues nextBigger = rainPatternList[keys[date.Month][~bestMatchIndex >= keys[date.Month].Count ? ~bestMatchIndex - 1 : ~bestMatchIndex]];
                    RainPatternValues lastSmaller = rainPatternList[keys[date.Month][~bestMatchIndex == 0 ? 0 : ~bestMatchIndex - 1]];
                    
                    if (Math.Abs(nextBigger.sum - sum) < Math.Abs(lastSmaller.sum - sum)) return nextBigger.pattern[date.Day - 1] * sum;
                    return lastSmaller.pattern[date.Day - 1] * sum;
                }
            }
        }
    }
}
