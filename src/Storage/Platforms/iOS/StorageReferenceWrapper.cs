using Firebase.Storage;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Storage.Platforms.iOS.Extensions;
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
    public IStorageTransferTask PutBytes(byte[] bytes, IStorageMetadata metadata = null)
    {
        return PutData(NSData.FromArray(bytes), metadata);
    }

    private IStorageTransferTask PutData(NSData data, IStorageMetadata metadata = null)
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
    public IStorageTransferTask PutFile(string filePath, IStorageMetadata metadata = null)
    {
        return PutData(NSData.FromStream(File.Open(filePath, FileMode.Open)), metadata);
    }

    /// <inheritdoc/>
    public IStorageTransferTask PutStream(Stream stream, IStorageMetadata metadata = null)
    {
        return PutData(NSData.FromStream(stream), metadata);
    }

    /// <inheritdoc/>
    public async Task<IStorageMetadata> GetMetadataAsync()
    {
        return (await _wrapped.GetMetadataAsync()).ToAbstract();
    }

    /// <inheritdoc/>
    public async Task<IStorageMetadata> UpdateMetadataAsync(IStorageMetadata metadata)
    {
        return (await _wrapped.UpdateMetadataAsync(metadata.ToNative())).ToAbstract();
    }

    /// <inheritdoc/>
    public async Task<string> GetDownloadUrlAsync()
    {
        var uri = await _wrapped.GetDownloadUrlAsync();
        return uri.AbsoluteString;
    }

    /// <inheritdoc/>
    public Task<IStorageListResult> ListAsync(long maxResults)
    {
        var tcs = new TaskCompletionSource<IStorageListResult>();
        _wrapped.List(
            maxResults,
            (listResult, error) => {
                if(error == null) {
                    tcs.SetResult(listResult.ToAbstract());
                } else {
                    tcs.SetException(new FirebaseException(error.LocalizedDescription));
                }
            }
        );
        return tcs.Task;
    }

    /// <inheritdoc/>
    public Task<IStorageListResult> ListAllAsync()
    {
        var tcs = new TaskCompletionSource<IStorageListResult>();
        _wrapped.ListAll(
            (x, error) => {
                if(error == null) {
                    tcs.SetResult(x.ToAbstract());
                } else {
                    tcs.SetException(new FirebaseException(error.LocalizedDescription));
                }
            }
        );
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
                    tcs.SetResult(data.AsStream());
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
                    tcs.SetResult(data.ToArray());
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
    public IStorageTransferTask DownloadFile(string destinationPath)
    {
        var wrapper = new StorageTransferTaskWrapper<StorageDownloadTask, NSUrl>();
        wrapper.TransferTask = _wrapped.WriteToFile(
            NSUrl.FromFilename(destinationPath),
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
    public IStorageReference Parent => _wrapped.Parent?.ToAbstract();

    /// <inheritdoc/>
    public IStorageReference Root => _wrapped.Root.ToAbstract();

    /// <inheritdoc/>
    public string Bucket => _wrapped.Bucket;

    /// <inheritdoc/>
    public string Name => _wrapped.Name;

    /// <inheritdoc/>
    public string FullPath => $"/{_wrapped.FullPath}";
}