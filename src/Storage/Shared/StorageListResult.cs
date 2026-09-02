namespace Plugin.Firebase.Storage;

/// <summary>
/// In-memory implementation of <see cref="IStorageListResult"/> for composed results.
/// </summary>
internal sealed class PagedStorageListResult : IStorageListResult
{
    public PagedStorageListResult(
        IEnumerable<IStorageReference> items,
        IEnumerable<IStorageReference> prefixes,
        string? pageToken)
    {
        Items = items.ToList();
        Prefixes = prefixes.ToList();
        PageToken = pageToken;
    }

    public IEnumerable<IStorageReference> Items { get; }

    public IEnumerable<IStorageReference> Prefixes { get; }

    public string? PageToken { get; }
}