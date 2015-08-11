using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace local
{
    class CsvReader
    {
        private StreamReader stream;
        private IEnumerable<String> fieldNames;

        public CsvReader(Stream stream)
        {
            this.stream = new StreamReader(stream);
            fieldNames = parseLine();
        }

        public IDictionary<String, String> readLine()
        {
            if (stream.EndOfStream) return null;

            IEnumerable<String> fields = parseLine();
            return fieldNames.Zip(fields, (k, v) => new { k, v })
              .ToDictionary(x => x.k, x => x.v);
        }

        public bool EndOfStream()
        {
            return stream.EndOfStream;
        }

        private IEnumerable<String> parseLine()
        {
            IList<String> fields = stream.ReadLine().Split(';');
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
                result.Add(f);
            }

            return result.Select<String, String>(x => x.Replace("\\\" ", "\"").Replace("\\\\", "\\"));
        }
    }
}
