using Firebase.CloudFirestore;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

/// <summary>
/// Wraps native iOS Firestore snapshot metadata.
/// </summary>
public sealed class SnapshotMetadataWrapper : ISnapshotMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotMetadataWrapper"/> class.
    /// </summary>
    /// <param name="metadata">The native iOS snapshot metadata to wrap.</param>
    public SnapshotMetadataWrapper(SnapshotMetadata metadata)
    {
        HasPendingWrites = metadata.HasPendingWrites;
        IsFromCache = metadata.IsFromCache;
    }

    /// <inheritdoc/>
    public bool HasPendingWrites { get; }

    /// <inheritdoc/>
    public bool IsFromCache { get; }
}