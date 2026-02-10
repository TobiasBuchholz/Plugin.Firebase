using Plugin.Firebase.Core.Utils;

namespace Plugin.Firebase.Core.Extensions;

/// <summary>
/// Extensions for working with <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Null-safe sequence equality comparison.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="this">First sequence.</param>
    /// <param name="other">Second sequence.</param>
    /// <param name="comparer">Optional element equality comparer (pass <c>null</c> to use the default comparer).</param>
    /// <returns><c>true</c> if both sequences are equal; otherwise <c>false</c>.</returns>
    public static bool SequenceEqualSafe<T>(this IEnumerable<T>? @this, IEnumerable<T>? other, Func<T, T, bool>? comparer = null)
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

    /// <summary>
    /// Sequence equality comparison using an optional element comparer.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="source">First sequence.</param>
    /// <param name="other">Second sequence.</param>
    /// <param name="comparer">Optional element equality comparer (pass <c>null</c> to use the default comparer).</param>
    /// <returns><c>true</c> if both sequences are equal; otherwise <c>false</c>.</returns>
    public static bool SequenceEqual<T>(this IEnumerable<T> source, IEnumerable<T> other, Func<T, T, bool>? comparer)
    {
        return comparer == null
            ? source.SequenceEqual(other)
            : source.SequenceEqual(other, new FuncEqualityComparer<T>(comparer));
    }
}