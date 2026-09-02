namespace Plugin.Firebase.Storage;

/// <summary>
/// Class which represents the metadata on an object in Firebase Storage. This metadata is returned on successful operations, and can be
/// used to retrieve download URLs, content types, and a storage reference to the object in question.
/// </summary>
public interface IStorageMetadata
{
    /// <summary>
    /// The name of the bucket containing this object, or <see langword="null"/> when unavailable.
    /// </summary>
    string? Bucket { get; }

    /// <summary>
    /// The content generation of this object, or <see langword="null"/> when unavailable. Used for object versioning.
    /// </summary>
    long? Generation { get; }

    /// <summary>
    /// The version of the metadata for this object at this generation. Used for preconditions and for detecting changes in metadata.
    /// A metageneration number is only meaningful in the context of a particular generation of a particular object.
    /// The value is <see langword="null"/> when unavailable.
    /// </summary>
    long? MetaGeneration { get; }

    /// <summary>
    /// The name of this object, or <see langword="null"/> when unavailable. In gs://bucket/path/to/object.txt, this is object.txt.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// The full path of this object, or <see langword="null"/> when unavailable. In gs://bucket/path/to/object.txt, this is path/to/object.txt.
    /// </summary>
    string? Path { get; }

    /// <summary>
    /// Content-Length of the data in bytes.
    /// </summary>
    long Size { get; }

    /// <summary>
    /// Cache-Control directive for the object data, or <see langword="null"/> when unset.
    /// </summary>
    string? CacheControl { get; }

    /// <summary>
    /// Content-Disposition of the object data, or <see langword="null"/> when unset.
    /// </summary>
    string? ContentDisposition { get; }

    /// <summary>
    /// Content-Encoding of the object data, or <see langword="null"/> when unset.
    /// </summary>
    string? ContentEncoding { get; }

    /// <summary>
    /// Content-Language of the object data, or <see langword="null"/> when unset.
    /// </summary>
    string? ContentLanguage { get; }

    /// <summary>
    /// Content-Type of the object data, or <see langword="null"/> when unset.
    /// </summary>
    string? ContentType { get; }

    /// <summary>
    /// User-provided metadata in key/value pairs, or <see langword="null"/> when none is provided.
    /// </summary>
    IDictionary<string, string>? CustomMetadata { get; }

    /// <summary>
    /// MD5 hash of the data encoded using base64, or <see langword="null"/> when unavailable.
    /// </summary>
    string? MD5Hash { get; }

    /// <summary>
    /// A reference to the object in Firebase Storage, or <see langword="null"/> when the native SDK does not expose one.
    /// The current iOS Firebase SDK does not expose this value.
    /// </summary>
    IStorageReference? StorageReference { get; }

    /// <summary>
    /// The time the <c>IStorageReference</c> was created, or <see langword="null"/> when unavailable.
    /// </summary>
    DateTimeOffset? CreationTime { get; }

    /// <summary>
    /// The time the <c>IStorageReference</c> was last updated, or <see langword="null"/> when unavailable.
    /// </summary>
    DateTimeOffset? UpdatedTime { get; }
}