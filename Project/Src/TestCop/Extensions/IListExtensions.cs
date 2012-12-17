using System;
using System.Collections.Generic;
using System.Linq;

namespace TestCop.Extensions
{
    public static class ListExtensions
    {   
        public static void AddIfMissing<T>(this IList<T> list, T value)
        {
            if(!list.Contains(value))list.Add(value);
        }

        public static void AddIfMissing<T>(this IList<T> list, T value, Func<T,T,bool> matcher)
        {
            if (list.Any(item => matcher(item,value)))
            {
                return;
            }
            list.Add(value);
        }

        public static void AddRangeIfMissing<T>(this IList<T> list, IEnumerable<T> newItems, Func<T, T, bool> matcher)
        {
            foreach (var newItem in newItems)
            {
                list.AddIfMissing(newItem, matcher);
            }
        }

    }
}
