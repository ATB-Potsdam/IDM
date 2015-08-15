using System;
using System.Linq;
using System.Reflection;

using atbApi.data;

namespace local
{
    internal static class Tools
    {
        internal static double Linear_day(int iterator, int startIterator, int endIterator, double startValue, double endValue)
        {
            return (iterator - startIterator) * (endValue - startValue) / (endIterator - startIterator + 1) + startValue;
        }

        internal static T MergeObjects<T>(T target, T source)
        {
            if (target == null) return source;
            if (source == null) return target;

            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

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
    }
}