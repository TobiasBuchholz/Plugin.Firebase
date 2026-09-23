using System.Runtime.CompilerServices;
using Firebase.CloudFirestore;
using Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

/// <summary>
/// Wraps a native iOS Firestore document snapshot with typed data.
/// </summary>
/// <typeparam name="T">The type to deserialize the document data into.</typeparam>
public sealed class DocumentSnapshotWrapper<T> : DocumentSnapshotWrapper, IDocumentSnapshot<T>
{
    // set on the first successful read; no lock is held while the model is built, and a failed conversion isn't
    // stored, so the next read converts again
    private StrongBox<T?>? _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentSnapshotWrapper{T}"/> class.
    /// </summary>
    /// <param name="documentSnapshot">The native iOS document snapshot to wrap.</param>
    public DocumentSnapshotWrapper(DocumentSnapshot documentSnapshot)
        : base(documentSnapshot) { }

    /// <inheritdoc/>
    public new T? Data => (Volatile.Read(ref _data) ?? ConvertData()).Value;

    private StrongBox<T?> ConvertData()
    {
        var data = Wrapped.Data;
        var converted = new StrongBox<T?>(data == null ? default(T) : data.Cast<T>(Wrapped.Id));
        // when two threads convert at once, both return the instance that was stored first
        return Interlocked.CompareExchange(ref _data, converted, null) ?? converted;
    }
}

/// <summary>
/// Wraps a native iOS Firestore document snapshot.
/// </summary>
public class DocumentSnapshotWrapper : IDocumentSnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentSnapshotWrapper"/> class.
    /// </summary>
    /// <param name="snapshot">The native iOS document snapshot to wrap.</param>
    public DocumentSnapshotWrapper(DocumentSnapshot snapshot)
    {
        Wrapped = snapshot;
    }

    /// <inheritdoc/>
    public object? Data => Wrapped.Data;

    /// <inheritdoc/>
    public ISnapshotMetadata Metadata => Wrapped.Metadata.ToAbstract();

    /// <inheritdoc/>
    public IDocumentReference Reference => Wrapped.Reference.ToAbstract();

    /// <summary>
    /// Gets the underlying native iOS document snapshot.
    /// </summary>
    public DocumentSnapshot Wrapped { get; }
}