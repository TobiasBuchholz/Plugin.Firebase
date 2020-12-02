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
            return _wrapped.GetChild(path).ToAbstract();
        }

        public IStorageUploadTask PutBytes(byte[] bytes, IStorageMetadata metadata = null)
        {
            return PutData(NSData.FromArray(bytes), metadata);
        }
        
        private IStorageUploadTask PutData(NSData data, IStorageMetadata metadata = null)
        {
            var wrapper = new StorageUploadTaskWrapper();
            wrapper.UploadTask = _wrapped.PutData(data, metadata?.ToNative(), wrapper.CompletionHandler);
            return wrapper;
        }
        
        public IStorageUploadTask PutFile(string filePath, IStorageMetadata metadata = null)
        {
            return PutData(NSData.FromStream(File.Open(filePath, FileMode.Open)), metadata);
        }

        public IStorageUploadTask PutStream(Stream stream, IStorageMetadata metadata = null)
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
            return WebUtility.UrlDecode(uri.AbsoluteString);
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

        public Task<bool> DownloadFileAsync(string destinationPath)
        {
            var tcs = new TaskCompletionSource<bool>();
            _wrapped.WriteToFile(NSUrl.FromString(destinationPath), (url, error) => {
                if(error == null && url != null) {
                    tcs.SetResult(true);
                } else {
                    tcs.SetException(new FirebaseException(error?.LocalizedDescription ?? "NSUrl is null"));
                }
            });
            return tcs.Task;
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
}