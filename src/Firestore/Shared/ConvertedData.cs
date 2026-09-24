namespace Plugin.Firebase.Firestore;

/// <summary>
/// Holds a document snapshot's data after it has been converted to <typeparamref name="T"/>, so the conversion runs
/// once per snapshot.
/// </summary>
/// <remarks>
/// A field of this type is null until the data has been converted, which keeps "not converted yet" apart from converted
/// data that is itself null.
/// </remarks>
/// <typeparam name="T">The type the data is converted to.</typeparam>
internal sealed class ConvertedData<T>
{
    private ConvertedData(T? value)
    {
        Value = value;
    }

    public T? Value { get; }

    /// <summary>
    /// Returns the data held in <paramref name="field"/>, converting it with <paramref name="convert"/> first if nothing is
    /// held yet.
    /// </summary>
    /// <remarks>
    /// No lock is held while <paramref name="convert"/> runs, and a failed conversion isn't stored, so the next call
    /// converts again. When two threads convert at once, both get the data that was stored first.
    /// </remarks>
    public static T? GetOrConvert<TState>(ref ConvertedData<T>? field, TState state, Func<TState, T?> convert)
    {
        var converted = Volatile.Read(ref field);
        if(converted == null) {
            var created = new ConvertedData<T>(convert(state));
            converted = Interlocked.CompareExchange(ref field, created, null) ?? created;
        }
        return converted.Value;
    }
}