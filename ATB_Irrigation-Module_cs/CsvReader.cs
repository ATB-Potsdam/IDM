/*!
 * \file    CsvReader.cs
 *
 * \brief   Implements the CSV reader class.
 *
 * \author  Hantigk
 * \date    13.08.2015
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace local
{
    /*!
     * \brief   A CSV reader.
     *
     */

    internal class CsvReader
    {
        private StreamReader stream;
        private IEnumerable<String> fieldNames;

        /*!
         * \brief   Constructor, parses first line and initializes the fieldNames
         *
         * \param   stream  A readable stream to read csv data from, line by line
         */

        internal CsvReader(Stream stream)
        {
            this.stream = new StreamReader(Tools.TryUnzip(stream));
            fieldNames = parseLine();
        }

        /*!
         * \brief   Reads one line with parseLine() function and maps the strings to the fields as Dictionary<String, String>
         *
         * \return  Dictionary<String, String>
         */

        internal IDictionary<String, String> readLine()
        {
            if (stream.EndOfStream) return null;

            IEnumerable<String> fields = parseLine();
            if (fields == null) return null;

            return fieldNames.Zip(fields, (k, v) => new { k, v })
              .ToDictionary(x => x.k, x => x.v);
        }

        /*!
         * \brief   check if stream is at end
         *
         * \return  true if stream is at end, false otherwise
         */

        internal bool EndOfStream()
        {
            return stream.EndOfStream;
        }

        /*!
         * \brief   reads one line from stream, split it into subStrings, trims and process escape sequences
         *
         * \return  sorted list with strings
         */

        private IEnumerable<String> parseLine()
        {
            String line = stream.ReadLine();
            if (line.Length < 4 || line[0] == '#') return null;

            IList<String> fields = line.Split(';');
            IList<String> result = new List<String>();
            bool inString = false;
            foreach (String field in fields)
            {
                String f = field;
                bool stringStart = !inString && f.StartsWith("\"");
                if (stringStart) f = f.Substring(1);
                f = f.Replace("\\", "\\\\").Replace("\"\"", "\\\" ");
                bool stringEnd = f.EndsWith("\"");
                if (stringEnd) f = f.TrimEnd('"');
                if (inString)
                {
                    inString = !stringEnd;
                    result[result.Count - 1] += ";" + f;
                    continue;
                }
                inString = stringStart && !stringEnd;
                //if (!result.Contains(f)) result.Add(f);
                result.Add(f);
            }

            return result.Select<String, String>(x => x.Replace("\\\" ", "\"").Replace("\\\\", "\\"));
        }
    }
}
