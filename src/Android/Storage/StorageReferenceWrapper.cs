using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Storage;
using Plugin.Firebase.Storage;
using AndroidUri = Android.Net.Uri;
using AndroidTask = Android.Gms.Tasks.Task;
using File = Java.IO.File;

namespace Plugin.Firebase.Android.Storage
{
    public sealed class StorageReferenceWrapper : IStorageReference
    {
        private readonly StorageReference _reference;
        
        public StorageReferenceWrapper(StorageReference reference)
        {
            _reference = reference;
        }

        public IStorageReference GetChild(string path)
        {
            return new StorageReferenceWrapper(_reference.Child(path));
        }

        public IStorageUploadTask PutBytes(byte[] bytes)
        {
            return new StorageUploadTaskWrapper(_reference.PutBytes(bytes));
        }

        public IStorageUploadTask PutFile(string filePath)
        {
            return new StorageUploadTaskWrapper(_reference.PutFile(AndroidUri.FromFile(new File(filePath))));
        }

        public IStorageUploadTask PutStream(Stream stream)
        {
            return new StorageUploadTaskWrapper(_reference.PutStream(stream));
        }

        public async Task<string> GetDownloadUrlAsync()
        {
            var uri = await _reference.GetDownloadUrlAsync();
            return uri.ToString();
        }

        public IStorageReference Parent => new StorageReferenceWrapper(_reference.Parent);
        public IStorageReference Root => new StorageReferenceWrapper(_reference.Root);
        public string Bucket => _reference.Bucket;
        public string Name => _reference.Name;
        public string FullPath => _reference.Path;
    }
}