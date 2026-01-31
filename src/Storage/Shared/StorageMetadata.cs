namespace Plugin.Firebase.Storage;

/// <summary>
/// Represents metadata for an object stored in Firebase Storage.
/// </summary>
public sealed class StorageMetadata : IStorageMetadata
{
    /// <summary>
    /// Creates a new <c>StorageMetadata</c> instance.
    /// </summary>
    /// <param name="bucket">The name of the bucket containing this object.</param>
    /// <param name="generation">The content generation of this object.</param>
    /// <param name="metaGeneration">The version of the metadata for this object.</param>
    /// <param name="name">The name of this object.</param>
    /// <param name="path">The full path of this object.</param>
    /// <param name="size">Content-Length of the data in bytes.</param>
    /// <param name="cacheControl">Cache-Control directive for the object data.</param>
    /// <param name="contentDisposition">Content-Disposition of the object data.</param>
    /// <param name="contentEncoding">Content-Encoding of the object data.</param>
    /// <param name="contentLanguage">Content-Language of the object data.</param>
    /// <param name="contentType">Content-Type of the object data.</param>
    /// <param name="customMetadata">User-provided metadata in key/value pairs.</param>
    /// <param name="md5Hash">MD5 hash of the data, encoded using base64.</param>
    /// <param name="storageReference">A reference to the object in Firebase Storage.</param>
    /// <param name="updatedTime">The time the object was last updated.</param>
    /// <param name="creationTime">The time the object was created.</param>
    public StorageMetadata(
        string bucket = null,
        long generation = 0,
        long metaGeneration = 0,
        string name = null,
        string path = null,
        long size = 0,
        string cacheControl = null,
        string contentDisposition = null,
        string contentEncoding = null,
        string contentLanguage = null,
        string contentType = null,
        IDictionary<string, string> customMetadata = null,
        string md5Hash = null,
        IStorageReference storageReference = null,
        DateTimeOffset updatedTime = default(DateTimeOffset),
        DateTimeOffset creationTime = default(DateTimeOffset)
    )
    {
        Bucket = bucket;
        Generation = generation;
        MetaGeneration = metaGeneration;
        Name = name;
        Path = path;
        Size = size;
        CacheControl = cacheControl;
        ContentLanguage = contentLanguage;
        ContentType = contentType;
        ContentDisposition = contentDisposition;
        ContentEncoding = contentEncoding;
        CustomMetadata = customMetadata;
        MD5Hash = md5Hash;
        StorageReference = storageReference;
        CreationTime = updatedTime;
        UpdatedTime = creationTime;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[{nameof(StorageMetadata)}: {nameof(Path)}={Path}, {nameof(ContentType)}={ContentType}, {nameof(Size)}={Size}]";
    }

    /// <inheritdoc />
    public string Bucket { get; }

    /// <inheritdoc />
    public long Generation { get; }

    /// <inheritdoc />
    public long MetaGeneration { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Path { get; }

    /// <inheritdoc />
    public long Size { get; }

    /// <inheritdoc />
    public string CacheControl { get; }

    /// <inheritdoc />
    public string ContentDisposition { get; }

    /// <inheritdoc />
    public string ContentEncoding { get; }

    /// <inheritdoc />
    public string ContentLanguage { get; }

    /// <inheritdoc />
    public string ContentType { get; }

    /// <inheritdoc />
    public IDictionary<string, string> CustomMetadata { get; }

    /// <inheritdoc />
    public string MD5Hash { get; }

    /// <inheritdoc />
    public IStorageReference StorageReference { get; }

    /// <inheritdoc />
    public DateTimeOffset CreationTime { get; }

    /// <inheritdoc />
    public DateTimeOffset UpdatedTime { get; }
}