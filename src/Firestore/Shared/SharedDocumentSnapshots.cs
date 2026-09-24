namespace Plugin.Firebase.Firestore;

/// <summary>
/// Wraps the documents of one native query snapshot, so that its document list and its document changes share one
/// snapshot per document.
/// </summary>
/// <remarks>
/// The first list that is built wraps its documents without looking up any paths. Only when a second list is built does
/// a path cache get created, indexing the first list, so reading a single list never looks up a path. Native calls, such
/// as reading the document list or looking up a document's path, run outside the lock; it only guards the path cache.
/// <c>wrap</c> can run under the lock, so it must only create the wrapper.
/// </remarks>
/// <typeparam name="TNativeDocument">The native document snapshot type.</typeparam>
/// <typeparam name="TSnapshot">The snapshot handed out for each document.</typeparam>
internal sealed class SharedDocumentSnapshots<TNativeDocument, TSnapshot>
    where TSnapshot : class
{
    private readonly Func<IEnumerable<TNativeDocument>> _getNativeDocuments;
    private readonly Func<TNativeDocument, TSnapshot> _wrap;
    private readonly Func<TNativeDocument, string> _getPath;
    private readonly object _lock = new();
    private IReadOnlyList<TSnapshot>? _documents;
    // the first list that was built, kept without paths until a second list needs to share with it
    private WrappedList? _firstList;
    private Dictionary<string, TSnapshot>? _snapshotsByPath;

    public SharedDocumentSnapshots(
        Func<IEnumerable<TNativeDocument>> getNativeDocuments,
        Func<TNativeDocument, TSnapshot> wrap,
        Func<TNativeDocument, string> getPath
    )
    {
        _getNativeDocuments = getNativeDocuments;
        _wrap = wrap;
        _getPath = getPath;
    }

    /// <summary>
    /// The snapshots of the query's documents, built on the first read. Nothing is stored if the native call throws, so
    /// the next read tries again.
    /// </summary>
    public IReadOnlyList<TSnapshot> Documents {
        get {
            var documents = Volatile.Read(ref _documents);
            if(documents != null) {
                return documents;
            }

            var built = Share(_getNativeDocuments().ToList()).AsReadOnly();
            return Interlocked.CompareExchange(ref _documents, built, null) ?? built;
        }
    }

    /// <summary>
    /// Returns the snapshots for the documents of a change list, reusing the snapshot of any document that is in
    /// <see cref="Documents"/> or in another change list.
    /// </summary>
    public IReadOnlyList<TSnapshot> GetChangedDocuments(IReadOnlyList<TNativeDocument> nativeDocuments)
    {
        return Share(nativeDocuments);
    }

    private List<TSnapshot> Share(IReadOnlyList<TNativeDocument> nativeDocuments)
    {
        string[]? paths = null;
        string[]? firstListPaths = null;
        while(true) {
            WrappedList? firstListToIndex = null;
            lock(_lock) {
                if(_snapshotsByPath == null) {
                    if(_firstList == null) {
                        // nothing else has been built, so there is nothing to share with yet
                        var wrapped = nativeDocuments.Select(_wrap).ToList();
                        _firstList = new WrappedList(nativeDocuments, wrapped);
                        return wrapped;
                    }
                    if(paths != null && firstListPaths != null) {
                        // index the first list in the same step that creates the cache, so no other list can wrap one
                        // of its documents first
                        _snapshotsByPath = new Dictionary<string, TSnapshot>();
                        for(var i = 0; i < firstListPaths.Length; i++) {
                            _snapshotsByPath.Add(firstListPaths[i], _firstList.Snapshots[i]);
                        }
                        _firstList = null;
                    } else {
                        firstListToIndex = _firstList;
                    }
                }
                if(_snapshotsByPath != null && paths != null) {
                    return nativeDocuments.Select((x, i) => GetOrAdd(paths[i], x)).ToList();
                }
            }
            // another list exists, so look up the paths outside the lock and try again
            paths ??= nativeDocuments.Select(_getPath).ToArray();
            if(firstListToIndex != null) {
                firstListPaths = firstListToIndex.NativeDocuments.Select(_getPath).ToArray();
            }
        }
    }

    private TSnapshot GetOrAdd(string path, TNativeDocument nativeDocument)
    {
        if(!_snapshotsByPath!.TryGetValue(path, out var snapshot)) {
            snapshot = _wrap(nativeDocument);
            _snapshotsByPath.Add(path, snapshot);
        }
        return snapshot;
    }

    private sealed record WrappedList(IReadOnlyList<TNativeDocument> NativeDocuments, IReadOnlyList<TSnapshot> Snapshots);
}