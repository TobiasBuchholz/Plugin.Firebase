using System;
using System.Collections.Generic;
using System.Linq;
using Plugin.Firebase.Shared.Common.Utils;

namespace Plugin.Firebase.Shared.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool SequenceEqualSafe<T>(this IEnumerable<T> @this, IEnumerable<T> other, Func<T, T, bool> comparer = null)
        {
            if(@this == null && other == null) {
                return true;
            } else if(@this == null) {
                return false;
            } else if(other == null) {
                return false;
            } else {
                return @this.SequenceEqual(other, comparer);
            }
        }

        public static bool SequenceEqual<T>(this IEnumerable<T> source, IEnumerable<T> other, Func<T, T, bool> comparer)
        {
            return comparer == null ? source.SequenceEqual(other) : source.SequenceEqual(other, new FuncEqualityComparer<T>(comparer));
        }
    }
}