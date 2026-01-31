namespace Plugin.Firebase.Core.Utils;

/// <summary>
/// <see cref="IEqualityComparer{T}"/> implementation backed by delegate(s).
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public class FuncEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> _comparer;
    private readonly Func<T, int> _hash;

    /// <summary>
    /// Creates a new instance using the provided equality comparer and a constant hash code.
    /// </summary>
    /// <param name="comparer">Equality comparer.</param>
    public FuncEqualityComparer(Func<T, T, bool> comparer)
        : this(comparer, _ => 0)
    {
    }

    /// <summary>
    /// Creates a new instance using the provided equality comparer and hash function.
    /// </summary>
    /// <param name="comparer">Equality comparer.</param>
    /// <param name="hash">Hash function.</param>
    public FuncEqualityComparer(Func<T, T, bool> comparer, Func<T, int> hash)
    {
        _comparer = comparer;
        _hash = hash;
    }

    /// <inheritdoc />
    public bool Equals(T x, T y)
    {
        return _comparer(x, y);
    }

    /// <inheritdoc />
    public int GetHashCode(T obj)
    {
        return _hash(obj);
    }
}