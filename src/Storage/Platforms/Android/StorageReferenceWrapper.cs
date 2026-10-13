using Android.Gms.Extensions;
using Firebase.Storage;
using AndroidUri = Android.Net.Uri;
using File = Java.IO.File;
using NativeStorageMetadata = Firebase.Storage.StorageMetadata;

namespace Plugin.Firebase.Storage.Platforms.Android;

public sealed class StorageReferenceWrapper : IStorageReference
{
    private readonly StorageReference _wrapped;

    public StorageReferenceWrapper(StorageReference reference)
    {
        _wrapped = reference;
    }

    public IStorageReference GetChild(string path)
    {
        return _wrapped.Child(path).ToAbstract();
    }

    public IStorageTransferTask PutBytes(byte[] bytes, IStorageMetadata? metadata = null)
    {
        return metadata == null
            ? _wrapped.PutBytes(bytes).ToAbstract()
            : _wrapped.PutBytes(bytes, metadata.ToNative()).ToAbstract();
    }

    public IStorageTransferTask PutFile(string filePath, IStorageMetadata? metadata = null)
    {
        var uri = AndroidUri.FromFile(new File(filePath))
            ?? throw new InvalidOperationException("Could not create a file URI for the storage upload.");

        return metadata == null
            ? _wrapped.PutFile(uri).ToAbstract()
            : _wrapped.PutFile(uri, metadata.ToNative()).ToAbstract();
    }

    public IStorageTransferTask PutStream(Stream stream, IStorageMetadata? metadata = null)
    {
        return metadata == null
            ? _wrapped.PutStream(stream).ToAbstract()
            : _wrapped.PutStream(stream, metadata.ToNative()).ToAbstract();
    }

    public async Task<IStorageMetadata> GetMetadataAsync()
    {
        var metadata = await WrapNativeAsync(_wrapped.GetMetadata().AsAsync<NativeStorageMetadata>());
        return metadata.ToAbstract();
    }

    public async Task<IStorageMetadata> UpdateMetadataAsync(IStorageMetadata metadata)
    {
        var updated = await WrapNativeAsync(
            _wrapped.UpdateMetadata(metadata.ToNative()).AsAsync<NativeStorageMetadata>()
        );
        return updated.ToAbstract();
    }

    public async Task<string> GetDownloadUrlAsync()
    {
        var uri = await WrapNativeAsync(_wrapped.GetDownloadUrlAsync());
        return uri.ToString()
            ?? throw new InvalidOperationException("Firebase Storage returned a null download URL string.");
    }

    public async Task<IStorageListResult> ListAsync(long maxResults)
    {
        var result = await WrapNativeAsync(_wrapped.List((int) maxResults).AsAsync<ListResult>());
        return result.ToAbstract();
    }

    public async Task<IStorageListResult> ListAllAsync()
    {
        var result = await WrapNativeAsync(_wrapped.ListAll().AsAsync<ListResult>());
        return result.ToAbstract();
    }

    public async Task<Stream> GetStreamAsync(long maxSize)
    {
        return await DownloadToMemoryAsync(maxSize);
    }

    public async Task<byte[]> GetBytesAsync(long maxDownloadSizeBytes)
    {
        using var contents = await DownloadToMemoryAsync(maxDownloadSizeBytes);
        return contents.ToArray();
    }

    private async Task<MemoryStream> DownloadToMemoryAsync(long maxSize)
    {
        var processor = new DownloadStreamProcessor(maxSize);
        MemoryStream contents;
        try {
            await WrapNativeAsync(_wrapped.GetStream(processor));
            contents = processor.Contents;
        }
        finally {
            processor.Dispose();
        }
        contents.Position = 0;
        return contents;
    }

    public IStorageTransferTask DownloadFile(string destinationPath)
    {
        var uri = AndroidUri.Parse(destinationPath)
            ?? throw new InvalidOperationException("Could not parse the destination path as a URI.");

        return _wrapped.GetFile(uri).ToAbstract();
    }

    public async Task DeleteAsync()
    {
        await WrapNativeAsync(_wrapped.DeleteAsync());
    }

    private static async Task<T> WrapNativeAsync<T>(Task<T> task)
    {
        try {
            return await task;
        } catch(Java.Lang.Throwable e) {
            throw e.ToStorageException();
        }
    }

    private static async Task<Java.Lang.Object?> WrapNativeAsync(global::Android.Gms.Tasks.Task task)
    {
        try {
            return await task;
        } catch(Java.Lang.Throwable e) {
            throw e.ToStorageException();
        }
    }

    private static async Task WrapNativeAsync(Task task)
    {
        try {
            await task;
        } catch(Java.Lang.Throwable e) {
            throw e.ToStorageException();
        }
    }

    public IStorageReference? Parent => _wrapped.Parent?.ToAbstract();
    public IStorageReference Root => _wrapped.Root.ToAbstract();
    public string Bucket => _wrapped.Bucket;
    public string Name => _wrapped.Name;
    public string FullPath => _wrapped.Path;
}