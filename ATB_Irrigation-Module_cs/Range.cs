using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace local
{
    internal class Range<TValue, TPayload>
            where TValue : IComparable<TValue>
    {
        public TValue Min { get; set; }
        public TValue Max { get; set; }
        public TPayload Value { get; set; }

        public Range(TValue min, TValue max, TPayload value)
        {
            this.Min = min;
            this.Max = max;
            this.Value = value;
        }
    }

    internal class RangeComparer<TValue, TPayload>
        where TValue : IComparable<TValue>
    {
        private int Compare(Range<TValue, TPayload> range, TValue value)
        {
            if (range.Min.CompareTo(value) > 0)
                return 1;

            if (range.Max.CompareTo(value) < 0)
                return -1;

            return 0;
        }

        public int RangeIndexSearch(IList<Range<TValue, TPayload>> ranges, TValue value)
        {
            int min = 0;
            int max = ranges.Count - 1;

            while (min <= max)
            {
                int mid = (min + max) / 2;
                int comparison = this.Compare(ranges[mid], value);
                if (comparison == 0)
                {
                    return mid;
                }
                if (comparison < 0)
                {
                    min = mid + 1;
                }
                else if (comparison > 0)
                {
                    max = mid - 1;
                }
            }
            return ~min;
        }
    }
}
