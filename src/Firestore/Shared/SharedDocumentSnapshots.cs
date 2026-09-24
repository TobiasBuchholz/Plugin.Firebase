namespace Plugin.Firebase.Firestore;

/// <summary>
/// Wraps the documents of one native query snapshot, so that its document list and its document changes share one
/// snapshot per document.
/// </summary>
/// <remarks>
/// Native calls, such as reading the document list or looking up a document's path, run outside the lock; it only guards
/// publishing the document list and the path cache. <c>wrap</c> runs under the lock, so it must only create the wrapper.
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
    private IReadOnlyList<TNativeDocument>? _nativeDocuments;
    private IReadOnlyList<TSnapshot>? _documents;
    // created with the first change list, so reading only the document list never looks up a path
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
    /// The snapshots of the query's documents, built on the first read. Building fails without storing anything if the
    /// native call throws, so the next read tries again.
    /// </summary>
    public IReadOnlyList<TSnapshot> Documents {
        get {
            var documents = Volatile.Read(ref _documents);
            if(documents != null) {
                return documents;
            }

            var nativeDocuments = _getNativeDocuments().ToList();
            var wrapped = nativeDocuments.Select(_wrap).ToList().AsReadOnly();
            string[]? paths = null;
            while(true) {
                lock(_lock) {
                    if(_documents != null) {
                        return _documents;
                    }
                    if(_snapshotsByPath == null) {
                        // no change list has been built yet, so there is nothing to share with
                        _nativeDocuments = nativeDocuments;
                        return _documents = wrapped;
                    }
                    if(paths != null) {
                        return _documents = nativeDocuments.Select((x, i) => GetOrAdd(paths[i], x)).ToList().AsReadOnly();
                    }
                }
                // a change list was built meanwhile, so look up the paths and try again
                paths = nativeDocuments.Select(_getPath).ToArray();
            }
        }
    }

    /// <summary>
    /// Returns the snapshots for the documents of a change list, reusing the snapshot of any document that is in
    /// <see cref="Documents"/> or that another change list has already wrapped.
    /// </summary>
    public IReadOnlyList<TSnapshot> GetChangedDocuments(IReadOnlyList<TNativeDocument> nativeDocuments)
    {
        var paths = nativeDocuments.Select(_getPath).ToArray();
        string[]? documentPaths = null;
        while(true) {
            IReadOnlyList<TNativeDocument>? documentsToIndex;
            lock(_lock) {
                if(_snapshotsByPath == null && (_documents == null || documentPaths != null)) {
                    // index the document list in the same step that creates the cache, so no change can wrap one of
                    // its documents first
                    _snapshotsByPath = new Dictionary<string, TSnapshot>();
                    if(documentPaths != null) {
                        for(var i = 0; i < documentPaths.Length; i++) {
                            _snapshotsByPath.Add(documentPaths[i], _documents![i]);
                        }
                    }
                }
                if(_snapshotsByPath != null) {
                    return nativeDocuments.Select((x, i) => GetOrAdd(paths[i], x)).ToList();
                }
                documentsToIndex = _nativeDocuments;
            }
            documentPaths = documentsToIndex!.Select(_getPath).ToArray();
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
}