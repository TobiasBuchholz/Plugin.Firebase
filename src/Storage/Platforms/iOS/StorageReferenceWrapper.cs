using Firebase.Storage;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Storage.Platforms.iOS.Extensions;
using NativeStorageListResult = Firebase.Storage.StorageListResult;
using NativeStorageMetadata = Firebase.Storage.StorageMetadata;

namespace Plugin.Firebase.Storage.Platforms.iOS;

/// <summary>
/// Wraps a native iOS Firebase StorageReference to implement IStorageReference.
/// </summary>
public sealed class StorageReferenceWrapper : IStorageReference
{
    private readonly StorageReference _wrapped;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageReferenceWrapper"/> class.
    /// </summary>
    /// <param name="reference">The native iOS storage reference to wrap.</param>
    public StorageReferenceWrapper(StorageReference reference)
    {
        _wrapped = reference;
    }

    /// <inheritdoc/>
    public IStorageReference GetChild(string path)
    {
        return _wrapped.GetChild(path).ToAbstract();
    }

    /// <inheritdoc/>
    public IStorageTransferTask PutBytes(byte[] bytes, IStorageMetadata? metadata = null)
    {
        var data = NSData.FromArray(bytes)
            ?? throw new InvalidOperationException("Could not create upload data from the supplied bytes.");

        return PutData(data, metadata);
    }

    private IStorageTransferTask PutData(NSData data, IStorageMetadata? metadata = null)
    {
        var wrapper = new StorageTransferTaskWrapper<StorageUploadTask, NativeStorageMetadata>();
        wrapper.TransferTask = _wrapped.PutData(
            data,
            metadata?.ToNative(),
            (x, e) => wrapper.CompletionHandler(x, e)
        );
        return wrapper;
    }

    /// <inheritdoc/>
    public IStorageTransferTask PutFile(string filePath, IStorageMetadata? metadata = null)
    {
        using var fileStream = File.Open(filePath, FileMode.Open);
        return PutData(CreateData(fileStream), metadata);
    }

    /// <inheritdoc/>
    public IStorageTransferTask PutStream(Stream stream, IStorageMetadata? metadata = null)
    {
        return PutData(CreateData(stream), metadata);
    }

    private static NSData CreateData(Stream stream)
    {
        return NSData.FromStream(stream)
            ?? throw new InvalidOperationException("Could not create upload data from the supplied stream.");
    }

    /// <inheritdoc/>
    public async Task<IStorageMetadata> GetMetadataAsync()
    {
        var metadata = await _wrapped.GetMetadataAsync()
            ?? throw new InvalidOperationException("Firebase Storage returned null metadata.");

        return metadata.ToAbstract();
    }

    /// <inheritdoc/>
    public async Task<IStorageMetadata> UpdateMetadataAsync(IStorageMetadata metadata)
    {
        var updatedMetadata = await _wrapped.UpdateMetadataAsync(metadata.ToNative())
            ?? throw new InvalidOperationException("Firebase Storage returned null metadata.");

        return updatedMetadata.ToAbstract();
    }

    /// <inheritdoc/>
    public async Task<string> GetDownloadUrlAsync()
    {
        var uri = await _wrapped.GetDownloadUrlAsync();
        return uri.AbsoluteString
            ?? throw new InvalidOperationException("Firebase Storage returned a null download URL string.");
    }

    /// <inheritdoc/>
    public Task<IStorageListResult> ListAsync(long maxResults)
    {
        var tcs = new TaskCompletionSource<IStorageListResult>();
        _wrapped.List(
            maxResults,
            (listResult, error) => {
                if(error == null && listResult != null) {
                    tcs.SetResult(listResult.ToAbstract());
                } else {
                    tcs.SetException(
                        new FirebaseException(
                            error?.LocalizedDescription ?? "Firebase Storage returned a null list result."
                        )
                    );
                }
            }
        );
        return tcs.Task;
    }

    /// <inheritdoc/>
    public Task<IStorageListResult> ListAllAsync()
    {
        return ListAllPagedAsync();
    }

    private async Task<IStorageListResult> ListAllPagedAsync()
    {
        const long maxResultsPerPage = 1000;

        var items = new List<IStorageReference>();
        var prefixes = new List<IStorageReference>();
        string? pageToken = null;

        do {
            var page = await ListPageAsync(maxResultsPerPage, pageToken);
            items.AddRange(page.Items);
            prefixes.AddRange(page.Prefixes);
            pageToken = page.PageToken;
        } while(!string.IsNullOrEmpty(pageToken));

        return new PagedStorageListResult(items, prefixes, pageToken: null);
    }

    private Task<IStorageListResult> ListPageAsync(long maxResults, string? pageToken)
    {
        var tcs = new TaskCompletionSource<IStorageListResult>();

        void CompletionHandler(NativeStorageListResult? listResult, NSError? error)
        {
            if(error == null && listResult != null) {
                tcs.SetResult(listResult.ToAbstract());
            } else {
                tcs.SetException(
                    new FirebaseException(
                        error?.LocalizedDescription ?? "Firebase Storage returned a null list result."
                    )
                );
            }
        }

        if(string.IsNullOrEmpty(pageToken)) {
            _wrapped.List(maxResults, CompletionHandler);
        } else {
            _wrapped.List(maxResults, pageToken, CompletionHandler);
        }

        return tcs.Task;
    }

    /// <inheritdoc/>
    public Task<Stream> GetStreamAsync(long maxSize)
    {
        var tcs = new TaskCompletionSource<Stream>();
        _wrapped.GetData(
            maxSize,
            (data, error) => {
                if(error == null && data != null) {
                    if((long)data.Length > maxSize) {
                        tcs.SetException(CreateDownloadSizeExceededException(data.Length, maxSize));
                    } else {
                        tcs.SetResult(data.AsStream());
                    }
                } else {
                    tcs.SetException(
                        new FirebaseException(error?.LocalizedDescription ?? "Data is null")
                    );
                }
            }
        );
        return tcs.Task;
    }

    /// <inheritdoc/>
    public Task<byte[]> GetBytesAsync(long maxDownloadSizeBytes)
    {
        var tcs = new TaskCompletionSource<byte[]>();
        _wrapped.GetData(
            maxDownloadSizeBytes,
            (data, error) => {
                if(error == null && data != null) {
                    if((long)data.Length > maxDownloadSizeBytes) {
                        tcs.SetException(
                            CreateDownloadSizeExceededException(data.Length, maxDownloadSizeBytes)
                        );
                    } else {
                        tcs.SetResult(data.ToArray());
                    }
                } else {
                    tcs.SetException(
                        new FirebaseException(error?.LocalizedDescription ?? "Data is null")
                    );
                }
            }
        );
        return tcs.Task;
    }

    private static FirebaseException CreateDownloadSizeExceededException(nuint actualSize, long maxSize)
    {
        return new FirebaseException(
            $"The downloaded data ({actualSize} bytes) exceeds the maximum allowed size ({maxSize} bytes)."
        );
    }

    /// <inheritdoc/>
    public IStorageTransferTask DownloadFile(string destinationPath)
    {
        var url = NSUrl.FromFilename(destinationPath)
            ?? throw new InvalidOperationException("Could not create a file URL for the storage download.");

        var wrapper = new StorageTransferTaskWrapper<StorageDownloadTask, NSUrl>();
        wrapper.TransferTask = _wrapped.WriteToFile(
            url,
            (x, e) => wrapper.CompletionHandler(x, e)
        );
        return wrapper;
    }

    /// <inheritdoc/>
    public Task DeleteAsync()
    {
        return _wrapped.DeleteAsync();
    }

    /// <inheritdoc/>
    public IStorageReference? Parent => _wrapped.Parent?.ToAbstract();

    /// <inheritdoc/>
    public IStorageReference Root => _wrapped.Root.ToAbstract();

    /// <inheritdoc/>
    public string Bucket => _wrapped.Bucket;

    /// <inheritdoc/>
    public string Name => _wrapped.Name;

    /// <inheritdoc/>
    public string FullPath => $"/{_wrapped.FullPath}";
}