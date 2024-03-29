using Firebase.Storage;
using Foundation;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Storage.Platforms.iOS.Extensions;
using NativeStorageTaskStatus = Firebase.Storage.StorageTaskStatus;
using NativeStorageMetadata = Firebase.Storage.StorageMetadata;

namespace Plugin.Firebase.Storage.Platforms.iOS;

public sealed class StorageReferenceWrapper : IStorageReference
{
    private readonly StorageReference _wrapped;

    public StorageReferenceWrapper(StorageReference reference)
    {
        _wrapped = reference;
    }

    public IStorageReference GetChild(string path)
    {
        return _wrapped.GetChild(path).ToAbstract();
    }

    public IStorageTransferTask PutBytes(byte[] bytes, IStorageMetadata metadata = null)
    {
        return PutData(NSData.FromArray(bytes), metadata);
    }

    private IStorageTransferTask PutData(NSData data, IStorageMetadata metadata = null)
    {
        var wrapper = new StorageTransferTaskWrapper<StorageUploadTask, NativeStorageMetadata>();
        wrapper.TransferTask = _wrapped.PutData(data, metadata?.ToNative(), (x, e) => wrapper.CompletionHandler(x, e));
        return wrapper;
    }

    public IStorageTransferTask PutFile(string filePath, IStorageMetadata metadata = null)
    {
        return PutData(NSData.FromStream(File.Open(filePath, FileMode.Open)), metadata);
    }

    public IStorageTransferTask PutStream(Stream stream, IStorageMetadata metadata = null)
    {
        return PutData(NSData.FromStream(stream), metadata);
    }

    public async Task<IStorageMetadata> GetMetadataAsync()
    {
        return (await _wrapped.GetMetadataAsync()).ToAbstract();
    }

    public async Task<IStorageMetadata> UpdateMetadataAsync(IStorageMetadata metadata)
    {
        return (await _wrapped.UpdateMetadataAsync(metadata.ToNative())).ToAbstract();
    }

    public async Task<string> GetDownloadUrlAsync()
    {
        var uri = await _wrapped.GetDownloadUrlAsync();
        return uri.AbsoluteString;
    }

    public Task<IStorageListResult> ListAsync(long maxResults)
    {
        var tcs = new TaskCompletionSource<IStorageListResult>();
        _wrapped.List(maxResults, (listResult, error) => {
            if(error == null) {
                tcs.SetResult(listResult.ToAbstract());
            } else {
                tcs.SetException(new FirebaseException(error.LocalizedDescription));
            }
        });
        return tcs.Task;
    }

    public Task<IStorageListResult> ListAllAsync()
    {
        var tcs = new TaskCompletionSource<IStorageListResult>();
        _wrapped.ListAll((x, error) => {
            if(error == null) {
                tcs.SetResult(x.ToAbstract());
            } else {
                tcs.SetException(new FirebaseException(error.LocalizedDescription));
            }
        });
        return tcs.Task;
    }

    public Task<Stream> GetStreamAsync(long maxSize)
    {
        var tcs = new TaskCompletionSource<Stream>();
        _wrapped.GetData(maxSize, (data, error) => {
            if(error == null && data != null) {
                tcs.SetResult(data.AsStream());
            } else {
                tcs.SetException(new FirebaseException(error?.LocalizedDescription ?? "Data is null"));
            }
        });
        return tcs.Task;
    }


    public Task<byte[]> GetBytesAsync(long maxDownloadSizeBytes)
    {
        var tcs = new TaskCompletionSource<byte[]>();
        _wrapped.GetData(maxDownloadSizeBytes, (data, error) => {
            if(error == null && data != null) {
                tcs.SetResult(data.ToArray());
            } else {
                tcs.SetException(new FirebaseException(error?.LocalizedDescription ?? "Data is null"));
            }
        });
        return tcs.Task;
    }

    public IStorageTransferTask DownloadFile(string destinationPath)
    {
        var wrapper = new StorageTransferTaskWrapper<StorageDownloadTask, NSUrl>();
        wrapper.TransferTask = _wrapped.WriteToFile(NSUrl.FromFilename(destinationPath), (x, e) => wrapper.CompletionHandler(x, e));
        return wrapper;
    }

    public Task DeleteAsync()
    {
        return _wrapped.DeleteAsync();
    }

    public IStorageReference Parent => _wrapped.Parent?.ToAbstract();
    public IStorageReference Root => _wrapped.Root.ToAbstract();
    public string Bucket => _wrapped.Bucket;
    public string Name => _wrapped.Name;
    public string FullPath => $"/{_wrapped.FullPath}";
}
