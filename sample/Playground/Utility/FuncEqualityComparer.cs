namespace Utility;

public class FuncEqualityComparer<T> : IEqualityComparer<T>
{
    readonly Func<T, T, bool> _comparer;
    readonly Func<T, int> _hash;

    public FuncEqualityComparer(Func<T, T, bool> comparer)
        : this(comparer, t => 0)
    {
    }

    public FuncEqualityComparer(Func<T, T, bool> comparer, Func<T, int> hash)
    {
        _comparer = comparer;
        _hash = hash;
    }

    public bool Equals(T x, T y)
    {
        return _comparer(x, y);
    }

    public int GetHashCode(T obj)
    {
        return _hash(obj);
    }
}