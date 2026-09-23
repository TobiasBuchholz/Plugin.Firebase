using System.Runtime.CompilerServices;
using Firebase.Firestore;
using Plugin.Firebase.Firestore.Platforms.Android.Extensions;

namespace Plugin.Firebase.Firestore.Platforms.Android;

public sealed class DocumentSnapshotWrapper<T> : DocumentSnapshotWrapper, IDocumentSnapshot<T>
{
    // set on the first successful read; no lock is held while the model is built, and a failed conversion isn't
    // stored, so the next read converts again
    private StrongBox<T?>? _data;

    public DocumentSnapshotWrapper(DocumentSnapshot documentSnapshot)
        : base(documentSnapshot)
    {
    }

    public new T? Data => (Volatile.Read(ref _data) ?? ConvertData()).Value;

    private StrongBox<T?> ConvertData()
    {
        var data = Wrapped.Data;
        var converted = new StrongBox<T?>(data == null ? default(T) : data.Cast<T>(Wrapped.Id));
        // when two threads convert at once, both return the instance that was stored first
        return Interlocked.CompareExchange(ref _data, converted, null) ?? converted;
    }
}

public class DocumentSnapshotWrapper : IDocumentSnapshot
{
    public DocumentSnapshotWrapper(DocumentSnapshot snapshot)
    {
        Wrapped = snapshot;
    }

    public object? Data => Wrapped.Data;
    public ISnapshotMetadata Metadata => Wrapped.Metadata.ToAbstract();
    public IDocumentReference Reference => Wrapped.Reference.ToAbstract();
    public DocumentSnapshot Wrapped { get; }
}