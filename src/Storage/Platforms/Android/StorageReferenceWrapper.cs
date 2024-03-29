using Android.Gms.Extensions;
using Firebase.Storage;
using AndroidUri = Android.Net.Uri;
using AndroidTask = Android.Gms.Tasks.Task;
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

    public IStorageTransferTask PutBytes(byte[] bytes, IStorageMetadata metadata = null)
    {
        return metadata == null
            ? _wrapped.PutBytes(bytes).ToAbstract()
            : _wrapped.PutBytes(bytes, metadata.ToNative()).ToAbstract();
    }

    public IStorageTransferTask PutFile(string filePath, IStorageMetadata metadata = null)
    {
        return metadata == null
            ? _wrapped.PutFile(AndroidUri.FromFile(new File(filePath))).ToAbstract()
            : _wrapped.PutFile(AndroidUri.FromFile(new File(filePath)), metadata.ToNative()).ToAbstract();
    }

    public IStorageTransferTask PutStream(Stream stream, IStorageMetadata metadata = null)
    {
        return metadata == null
            ? _wrapped.PutStream(stream).ToAbstract()
            : _wrapped.PutStream(stream, metadata.ToNative()).ToAbstract();
    }

    public async Task<IStorageMetadata> GetMetadataAsync()
    {
        return (await _wrapped.GetMetadata().AsAsync<NativeStorageMetadata>()).ToAbstract();
    }

    public async Task<IStorageMetadata> UpdateMetadataAsync(IStorageMetadata metadata)
    {
        return (await _wrapped.UpdateMetadata(metadata.ToNative()).AsAsync<NativeStorageMetadata>()).ToAbstract();
    }

    public async Task<string> GetDownloadUrlAsync()
    {
        var uri = await _wrapped.GetDownloadUrlAsync();
        return uri.ToString();
    }

    public async Task<IStorageListResult> ListAsync(long maxResults)
    {
        return (await _wrapped.List((int) maxResults).AsAsync<ListResult>()).ToAbstract();
    }

    public async Task<IStorageListResult> ListAllAsync()
    {
        return (await _wrapped.ListAll().AsAsync<ListResult>()).ToAbstract();
    }

    public async Task<Stream> GetStreamAsync(long maxSize)
    {
        return (await _wrapped.GetStream(new StreamProcessor()).AsAsync<StreamDownloadTask.TaskSnapshot>()).Stream;
    }

    public async Task<byte[]> GetBytesAsync(long maxDownloadSizeBytes)
    {
        return (byte[]) await _wrapped.GetBytes(maxDownloadSizeBytes);
    }

    public IStorageTransferTask DownloadFile(string destinationPath)
    {
        return _wrapped.GetFile(AndroidUri.Parse(destinationPath)).ToAbstract();
    }

    public Task DeleteAsync()
    {
        return _wrapped.DeleteAsync();
    }

    public IStorageReference Parent => _wrapped.Parent?.ToAbstract();
    public IStorageReference Root => _wrapped.Root.ToAbstract();
    public string Bucket => _wrapped.Bucket;
    public string Name => _wrapped.Name;
    public string FullPath => _wrapped.Path;
}