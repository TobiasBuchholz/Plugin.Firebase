namespace Plugin.Firebase.Core.Utils;

public class FuncEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> _comparer;
    private readonly Func<T, int> _hash;

    public FuncEqualityComparer(Func<T, T, bool> comparer)
        : this(comparer, _ => 0)
    {
    }

    public FuncEqualityComparer(Func<T, T, bool> comparer, Func<T, int> hash)
    {
        _comparer = comparer;
        _hash = hash;
    }

    public bool Equals(T? x, T? y)
    {
        if(x is null && y is null) {
            return true;
        }
        if(x is null || y is null) {
            return false;
        }

        return _comparer(x, y);
    }

    public int GetHashCode(T obj)
    {
        return _hash(obj);
    }
}