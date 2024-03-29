namespace Plugin.Firebase.Storage;

/// <summary>
/// <c>IStorageReference</c> represents a reference to a Google Cloud Storage object. Developers can upload and download objects, as well
/// as get/set object metadata, and delete an object at the path.
/// </summary>
public interface IStorageReference
{
    /// <summary>
    /// Creates a new <c>IStorageReference</c> pointing to a child object of the current reference. path = foo child = bar newPath = foo/bar path
    /// = foo/bar child = baz newPath = foo/bar/baz. All leading and trailing slashes will be removed, and consecutive slashes will be compressed
    /// to single slashes. For example: child = /foo/bar newPath = foo/bar child = foo/bar/ newPath = foo/bar child = foo///bar newPath = foo/bar
    /// </summary>
    /// <param name="path">Path to append to the current path.</param>
    IStorageReference GetChild(string path);

    /// <summary>
    /// Uploads a byte array to firebase storage bucket.<br></br>
    /// CAUTION: Although this method returns an IStorageUploadTask, which has an AwaitAsync() method, the upload will start immediately on iOS
    /// </summary>
    IStorageTransferTask PutBytes(byte[] bytes, IStorageMetadata metaData = null);

    /// <summary>
    /// Uploads a file via it's file path to firebase storage bucket.<br></br>
    /// CAUTION: Although this method returns an IStorageUploadTask, which has an AwaitAsync() method, the upload will start immediately on iOS
    /// </summary>
    IStorageTransferTask PutFile(string filePath, IStorageMetadata metaData = null);

    /// <summary>
    /// Uploads a file via it's data stream to firebase storage bucket.<br></br>
    /// CAUTION: Although this method returns an IStorageUploadTask, which has an AwaitAsync() method, the upload will start immediately on iOS
    /// </summary>
    IStorageTransferTask PutStream(Stream stream, IStorageMetadata metaData = null);

    /// <summary>
    /// Retrieves metadata associated with an object at the current path.
    /// </summary>
    Task<IStorageMetadata> GetMetadataAsync();

    /// <summary>
    /// Updates the metadata associated with an object at the current path.
    /// </summary>
    /// <param name="metadata">An <c>IStorageMetaData</c> object with the metadata to update.</param>
    /// <returns></returns>
    Task<IStorageMetadata> UpdateMetadataAsync(IStorageMetadata metadata);

    /// <summary>
    /// Asynchronously retrieves a long lived download URL with a revocable token. This can be used to share the file with others, but can
    /// be revoked by a developer in the Firebase Console if desired.
    /// </summary>
    Task<string> GetDownloadUrlAsync();

    /// <summary>
    /// List up to maxResults items (files) and prefixes (folders) under this <c>IStorageReference</c>. “/” is treated as a path delimiter.
    /// Firebase Storage does not support unsupported object paths that end with “/” or contain two consecutive “/"s. All invalid objects
    /// in GCS will be filtered. <c>ListAsync(maxResults)</c> is only available for projects using Firebase Rules Version 2.
    /// </summary>
    /// <param name="maxResults">The maximum number of results to return in a single page. Must be greater than 0 and at most 1000.</param>
    Task<IStorageListResult> ListAsync(long maxResults);

    /// <summary>
    /// List all items (files) and prefixes (folders) under this <c>IStorageReference</c>. This is a helper method for calling
    /// <c>ListAsync()</c> repeatedly until there are no more results. Consistency of the result is not guaranteed if objects
    /// are inserted or removed while this operation is executing. All results are buffered in memory. <c>ListAllAsync()</c> is
    /// only available for projects using Firebase Rules Version 2.
    /// </summary>
    Task<IStorageListResult> ListAllAsync();

    /// <summary>
    /// Asynchronously downloads the object at the <c>IStorageReference</c> to memory. The provided max size will be allocated, so ensure
    /// that the device has enough free memory to complete the download. For downloading large files, <c>DownloadFile(destinationPath)</c>
    /// may be a better option.
    /// </summary>
    /// <param name="maxSize">
    /// The maximum size in bytes to download. If the download exceeds this size the task will be cancelled and an error will be returned.
    /// </param>
    Task<Stream> GetStreamAsync(long maxSize);

    /// <summary>
    /// Asynchronously downloads the object from this <c>IStorageReference</c>. A byte array will be allocated large enough to hold the entire file in memory.
    /// Therefore, using this method will impact memory usage of your process. If you are downloading many large files, getStream may be a better option.
    /// </summary>
    /// <param name="maxDownloadSizeBytes">
    /// The maximum allowed size in bytes that will be allocated. Set this parameter to prevent out of memory conditions from occurring. If the download exceeds this limit, the task will fail and an IndexOutOfBoundsException will be returned.    /// </param>
    Task<byte[]> GetBytesAsync(long maxDownloadSizeBytes);

    /// <summary>
    /// Asynchronously downloads the object at the current path to a specified system filepath.
    /// </summary>
    /// <param name="destinationPath">A file system URL representing the path the object should be downloaded to.</param>
    IStorageTransferTask DownloadFile(string destinationPath);

    /// <summary>
    /// Deletes the object at the current path.
    /// </summary>
    Task DeleteAsync();

    /// <summary>
    /// Creates a new <c>IStorageReference</c> pointing to the parent of the current reference or null if this instance references the root location.
    /// For example: path = foo/bar/baz parent = foo/bar path = foo parent = (root) path = (root) parent = null.
    /// </summary>
    IStorageReference Parent { get; }

    /// <summary>
    /// Creates a new <c>IStorageReference</c> pointing to the root object.
    /// </summary>
    IStorageReference Root { get; }

    /// <summary>
    /// The name of the Google Cloud Storage bucket associated with this reference, in gs://bucket/path/to/object.txt, the bucket would be: ‘bucket’.
    /// </summary>
    string Bucket { get; }

    /// <summary>
    /// The short name of the object associated with this reference, in gs://bucket/path/to/object.txt, the name of the object would be: ‘object.txt’
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The full path to this object, not including the Google Cloud Storage bucket. In gs://bucket/path/to/object.txt, the full path would be: ‘path/to/object.txt’
    /// </summary>
    string FullPath { get; }
}
