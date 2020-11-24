using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Storage;
using Foundation;
using Plugin.Firebase.Storage;
using NativeStorageTaskStatus = Firebase.Storage.StorageTaskStatus;

namespace Plugin.Firebase.iOS.Storage
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
            return new StorageReferenceWrapper(_reference.GetChild(path));
        }

        public IStorageUploadTask PutBytes(byte[] bytes)
        {
            return PutData(NSData.FromArray(bytes));
        }
        
        private IStorageUploadTask PutData(NSData data)
        {
            var wrapper = new StorageUploadTaskWrapper();
            wrapper.UploadTask = _reference.PutData(data, null, wrapper.CompletionHandler);
            return wrapper;
        }
        
        public IStorageUploadTask PutFile(string filePath)
        {
            return PutData(NSData.FromStream(File.Open(filePath, FileMode.Open)));
        }

        public IStorageUploadTask PutStream(Stream stream)
        {
            return PutData(NSData.FromStream(stream));
        }

        public async Task<string> GetDownloadUrlAsync()
        {
            var uri = await _reference.GetDownloadUrlAsync();
            return uri.AbsoluteString;
        }

        public IStorageReference Parent => new StorageReferenceWrapper(_reference.Parent);
        public IStorageReference Root => new StorageReferenceWrapper(_reference.Root);
        public string Bucket => _reference.Bucket;
        public string Name => _reference.Name;
        public string FullPath => _reference.FullPath;
    }
}