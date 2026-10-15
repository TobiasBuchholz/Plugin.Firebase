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
        var metadata = await StorageExceptionFactory.Wrap(
            () => _wrapped.GetMetadata().AsAsync<NativeStorageMetadata>()
        );
        return metadata.ToAbstract();
    }

    public async Task<IStorageMetadata> UpdateMetadataAsync(IStorageMetadata metadata)
    {
        var updated = await StorageExceptionFactory.Wrap(
            () => _wrapped.UpdateMetadata(metadata.ToNative()).AsAsync<NativeStorageMetadata>()
        );
        return updated.ToAbstract();
    }

    public async Task<string> GetDownloadUrlAsync()
    {
        var uri = await StorageExceptionFactory.Wrap(() => _wrapped.GetDownloadUrlAsync());
        return uri.ToString()
            ?? throw new InvalidOperationException("Firebase Storage returned a null download URL string.");
    }

    public Task<IStorageListResult> ListAsync(long maxResults)
    {
        // Validating first also keeps the int cast below from truncating a value outside the native range.
        StorageListArguments.ThrowIfMaxResultsOutOfRange(maxResults);
        return StorageExceptionFactory.Wrap(
            async () => (await _wrapped.List((int) maxResults).AsAsync<ListResult>()).ToAbstract()
        );
    }

    public async Task<IStorageListResult> ListAllAsync()
    {
        var result = await StorageExceptionFactory.Wrap(() => _wrapped.ListAll().AsAsync<ListResult>());
        return result.ToAbstract();
    }

    public async Task<Stream> GetStreamAsync(long maxSize)
    {
        var bytes = await GetBytesAsync(maxSize);
        return new MemoryStream(bytes, writable: false);
    }

    public async Task<byte[]> GetBytesAsync(long maxDownloadSizeBytes)
    {
        return (byte[]?) await StorageExceptionFactory.Wrap(async () => await _wrapped.GetBytes(maxDownloadSizeBytes))
            ?? throw new InvalidOperationException("Firebase Storage returned null download data.");
    }

    public IStorageTransferTask DownloadFile(string destinationPath)
    {
        var uri = AndroidUri.Parse(destinationPath)
            ?? throw new InvalidOperationException("Could not parse the destination path as a URI.");

        return _wrapped.GetFile(uri).ToAbstract();
    }

    public Task DeleteAsync()
    {
        return StorageExceptionFactory.Wrap(() => _wrapped.DeleteAsync());
    }

    public IStorageReference? Parent => _wrapped.Parent?.ToAbstract();
    public IStorageReference Root => _wrapped.Root.ToAbstract();
    public string Bucket => _wrapped.Bucket;
    public string Name => _wrapped.Name;
    public string FullPath => _wrapped.Path;
}