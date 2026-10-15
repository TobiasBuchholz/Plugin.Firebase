namespace Plugin.Firebase.Storage;

/// <summary>
/// Validates <see cref="IStorageReference.ListAsync(long)"/> arguments against the page size limits that both native
/// SDKs enforce, so both platforms reject an invalid value with the same exception and Android never truncates it to
/// an <see cref="int"/>.
/// </summary>
internal static class StorageListArguments
{
    /// <summary>
    /// The smallest page size the native SDKs accept.
    /// </summary>
    public const long MinMaxResults = 1;

    /// <summary>
    /// The largest page size the native SDKs accept.
    /// </summary>
    public const long MaxMaxResults = 1000;

    /// <summary>
    /// Throws <see cref="ArgumentOutOfRangeException"/> when <paramref name="maxResults"/> is not between
    /// <see cref="MinMaxResults"/> and <see cref="MaxMaxResults"/> inclusive.
    /// </summary>
    public static void ThrowIfMaxResultsOutOfRange(long maxResults)
    {
        if(maxResults is < MinMaxResults or > MaxMaxResults) {
            throw new ArgumentOutOfRangeException(
                nameof(maxResults),
                maxResults,
                "maxResults must be between 1 and 1000 inclusive."
            );
        }
    }
}