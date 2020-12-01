using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Firebase.Storage;
using Foundation;
using Plugin.Firebase.Common;
using Plugin.Firebase.Storage;
using NativeStorageTaskStatus = Firebase.Storage.StorageTaskStatus;

namespace Plugin.Firebase.iOS.Storage
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
            return new StorageReferenceWrapper(_wrapped.GetChild(path));
        }

        public IStorageUploadTask PutBytes(byte[] bytes)
        {
            return PutData(NSData.FromArray(bytes));
        }
        
        private IStorageUploadTask PutData(NSData data)
        {
            var wrapper = new StorageUploadTaskWrapper();
            wrapper.UploadTask = _wrapped.PutData(data, null, wrapper.CompletionHandler);
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
            var uri = await _wrapped.GetDownloadUrlAsync();
            return WebUtility.UrlDecode(uri.AbsoluteString);
        }

        public Task<IStorageListResult> ListAsync(long maxResults)
        {
            var tcs = new TaskCompletionSource<IStorageListResult>();
            _wrapped.List(maxResults, (x, error) => {
                if(error == null) {
                    tcs.SetResult(new StorageListResultsWrapper(x));
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
                    tcs.SetResult(new StorageListResultsWrapper(x));
                } else {
                    tcs.SetException(new FirebaseException(error.LocalizedDescription));
                }
            });
            return tcs.Task;
        }

        public Task DeleteAsync()
        {
            return _wrapped.DeleteAsync();
        }

        public IStorageReference Parent => _wrapped.Parent == null ? null : new StorageReferenceWrapper(_wrapped.Parent);
        public IStorageReference Root => new StorageReferenceWrapper(_wrapped.Root);
        public string Bucket => _wrapped.Bucket;
        public string Name => _wrapped.Name;
        public string FullPath => $"/{_wrapped.FullPath}";
    }
}