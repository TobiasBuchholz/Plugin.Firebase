using System.Collections.ObjectModel;
using Utility;

namespace System.Linq;

public static class EnumerableExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> @this)
    {
        return new ObservableCollection<T>(@this);
    }

    public static bool TryFirst<T>(this IEnumerable<T> @this, Func<T, bool> filter, out T result)
    {
        result = default(T);
        foreach(var item in @this) {
            if(filter(item)) {
                result = item;
                return true;
            }
        }
        return false;
    }

    public static bool TryRemove<T>(this IList<T> @this, T item)
    {
        if(item != null && @this.Contains(item)) {
            @this.Remove(item);
            return true;
        }
        return false;
    }

    public static bool TryRemoveFirst<T>(this IList<T> @this, Func<T, bool> filter, out T result)
    {
        if(@this.TryFirst(filter, out result)) {
            @this.Remove(result);
            return true;
        }
        return false;
    }

    public static bool TryRemoveFirst<T>(this IList<T> @this, Func<T, bool> filter)
    {
        if(@this.TryFirst(filter, out var result)) {
            @this.Remove(result);
            return true;
        }
        return false;
    }

    public static void RemoveLast<T>(this IList<T> @this)
    {
        @this.RemoveAt(@this.Count - 1);
    }

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

    public static bool SequenceEqual<T>(this IEnumerable<T> source, IEnumerable<T> other, Func<T, T, bool> comparer = null)
    {
        return comparer == null ? source.SequenceEqual(other) : source.SequenceEqual(other, new FuncEqualityComparer<T>(comparer));
    }

    public static T[] ToSingleArray<T>(this T @this)
    {
        var array = new T[1];
        array[0] = @this;
        return array;
    }

    public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> @this, int n)
    {
        if(@this == null) {
            throw new ArgumentNullException(nameof(@this));
        }

        if(n < 0) {
            throw new ArgumentOutOfRangeException(nameof(n), $"{nameof(n)} must be 0 or greater");
        }

        var temp = new LinkedList<T>();
        foreach(var value in @this) {
            temp.AddLast(value);
            if(temp.Count > n) {
                temp.RemoveFirst();
            }
        }
        return temp;
    }
}