using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Firebase.Storage;
using Plugin.Firebase.Extensions;
using Plugin.Firebase.Storage;
using AndroidUri = Android.Net.Uri;
using AndroidTask = Android.Gms.Tasks.Task;
using File = Java.IO.File;
using StorageMetadata = Firebase.Storage.StorageMetadata;

namespace Plugin.Firebase.Android.Storage
{
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

        public IStorageUploadTask PutBytes(byte[] bytes, IStorageMetadata metadata = null)
        {
            return metadata == null 
                ? _wrapped.PutBytes(bytes).ToAbstract() 
                : _wrapped.PutBytes(bytes, metadata.ToNative()).ToAbstract();
        }

        public IStorageUploadTask PutFile(string filePath, IStorageMetadata metadata = null)
        {
            return metadata == null 
                ? _wrapped.PutFile(AndroidUri.FromFile(new File(filePath))).ToAbstract()
                : _wrapped.PutFile(AndroidUri.FromFile(new File(filePath)), metadata.ToNative()).ToAbstract();
        }

        public IStorageUploadTask PutStream(Stream stream, IStorageMetadata metadata = null)
        {
            return metadata == null
                ? _wrapped.PutStream(stream).ToAbstract()
                : _wrapped.PutStream(stream, metadata.ToNative()).ToAbstract();
        }

        public async Task<IStorageMetadata> GetMetadataAsync()
        {
            return (await _wrapped.GetMetadata().ToTask<StorageMetadata>()).ToAbstract();
        }

        public async Task<IStorageMetadata> UpdateMetadataAsync(IStorageMetadata metadata)
        {
            return (await _wrapped.UpdateMetadata(metadata.ToNative()).ToTask<StorageMetadata>()).ToAbstract();
        }

        public async Task<string> GetDownloadUrlAsync()
        {
            var uri = await _wrapped.GetDownloadUrlAsync();
            return WebUtility.UrlDecode(uri.ToString());
        }

        public async Task<IStorageListResult> ListAsync(long maxResults)
        {
            return (await _wrapped.List((int) maxResults).ToTask<ListResult>()).ToAbstract();
        }

        public async Task<IStorageListResult> ListAllAsync()
        {
            return (await _wrapped.ListAll().ToTask<ListResult>()).ToAbstract();
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
}