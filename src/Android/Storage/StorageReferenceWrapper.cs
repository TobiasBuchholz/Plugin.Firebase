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
            return new StorageReferenceWrapper(_wrapped.Child(path));
        }

        public IStorageUploadTask PutBytes(byte[] bytes)
        {
            return new StorageUploadTaskWrapper(_wrapped.PutBytes(bytes));
        }

        public IStorageUploadTask PutFile(string filePath)
        {
            return new StorageUploadTaskWrapper(_wrapped.PutFile(AndroidUri.FromFile(new File(filePath))));
        }

        public IStorageUploadTask PutStream(Stream stream)
        {
            return new StorageUploadTaskWrapper(_wrapped.PutStream(stream));
        }

        public async Task<string> GetDownloadUrlAsync()
        {
            var uri = await _wrapped.GetDownloadUrlAsync();
            return WebUtility.UrlDecode(uri.ToString());
        }

        public async Task<IStorageListResult> ListAsync(long maxResults)
        {
            return new StorageListResultWrapper(await _wrapped.List((int) maxResults).ToTask<ListResult>());
        }

        public async Task<IStorageListResult> ListAllAsync()
        {
            return new StorageListResultWrapper(await _wrapped.ListAll().ToTask<ListResult>());
        }

        public Task DeleteAsync()
        {
            return _wrapped.DeleteAsync();
        }

        public IStorageReference Parent => _wrapped.Parent == null ? null : new StorageReferenceWrapper(_wrapped.Parent);
        public IStorageReference Root => new StorageReferenceWrapper(_wrapped.Root);
        public string Bucket => _wrapped.Bucket;
        public string Name => _wrapped.Name;
        public string FullPath => _wrapped.Path;
    }
}