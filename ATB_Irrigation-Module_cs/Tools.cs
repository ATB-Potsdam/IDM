/*!
 * \file    Tools.cs
 *
 * \brief   Collection of static helper functions.
 *
 * \author  Hunstock
 * \date    23.07.2015
 */

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using Newtonsoft.Json;


using atbApi.data;

/*! 
 * \brief   namespace for only local (dll internal) used classes and structures
 * 
 */
namespace local
{

    /*!
     * \brief   Static class for tools.
     *
     */

    internal static class Tools
    {
        /*! List of types of the numerics in c#. */
        internal static readonly Type[] NumericTypes = new[]
        {
            typeof(int), typeof(double), typeof(decimal),
            typeof(long), typeof(short), typeof(sbyte),
            typeof(byte), typeof(ulong), typeof(ushort),
            typeof(uint), typeof(float)
        };

        /*!
         * \brief   Query if 'type' is numeric.
         *
         * \param   type    The type to check.
         *
         * \return  true if numeric, false if not.
         */

        internal static bool IsNumeric(Type type)
        {
            if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return IsNumeric(Nullable.GetUnderlyingType(type));
            }

            return NumericTypes.Contains(type);
        }

        internal static T Add<T, S>(T num1, S num2)
        {
            dynamic a = num1;
            dynamic b = num2;
            return a + b;
        }

        internal static Dictionary<TKey, TValue> CloneDictionary<TKey, TValue>(Dictionary<TKey, TValue> src)
        {
            Dictionary<TKey, TValue> dst = new Dictionary<TKey, TValue>(src.Count, src.Comparer);

            foreach (KeyValuePair<TKey, TValue> entry in src)
            {
                dst.Add(entry.Key, (TValue)entry.Value);
            }
            return dst;
        }
        
        internal static T Clone<T>(T source)
        {
            var ser = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(ser);
        }
        
        internal static double Linear_day(int iterator, int startIterator, int endIterator, double startValue, double endValue)
        {
            return (iterator - startIterator) * (endValue - startValue) / (endIterator - startIterator + 1) + startValue;
        }

        internal static T MergeObjects<T>(T target, T source)
        {
            if (target == null) return source;
            if (source == null) return target;

            Type t = typeof(T);

            //use this for .net 4.0
            //var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);
            //use this for .net 4.5
            var properties = t.GetRuntimeProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                if (value != null)
                    prop.SetValue(target, value, null);
            }
            return target;
        }

        internal static DateTime AdjustTimeStep(DateTime date, TimeStep step)
        {
            //adjust date to lower bound
            switch (step)
            {
                case TimeStep.hour:
                    return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0, 0, date.Kind);
                case TimeStep.day:
                    return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0, date.Kind);
                case TimeStep.month:
                    return new DateTime(date.Year, date.Month, 1, 0, 0, 0, 0, date.Kind);
            }
            return date;
        }

        internal static IEnumerable<DateTime> EachHour(DateTime start, DateTime end)
        {
            for (var date = start; date <= end; date = date.AddHours(1))
                yield return date;
        }
        internal static IEnumerable<DateTime> EachDay(DateTime start, DateTime end)
        {
            for (var date = start; date <= end; date = date.AddDays(1))
                yield return date;
        }
        internal static IEnumerable<DateTime> EachMonth(DateTime start, DateTime end)
        {
            for (var date = start; date <= end; date = date.AddMonths(1))
                yield return date;
        }
        internal static IEnumerable<DateTime> EachYear(DateTime start, DateTime end)
        {
            for (var date = start; date <= end; date = date.AddYears(1))
                yield return date;
        }
    
        internal static Stream TryUnzip(Stream fileStream)
        {
            Byte[] gzipSig = new Byte[3];
            //read gzip signature 0x1F 0x8B 0x08
            fileStream.Read(gzipSig, 0, gzipSig.Length);
            //reset stream to begin
            fileStream.Seek(0, SeekOrigin.Begin);
            //check if stream has gzip signature und unzip it
            if (System.Convert.ToBase64String(gzipSig).Equals("H4sI")) return new GZipStream(fileStream, CompressionMode.Decompress);
            
            return fileStream;
        }
    }
}