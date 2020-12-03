using System;
using System.IO;
using System.Threading.Tasks;

namespace Plugin.Firebase.Storage
{
    public interface IStorageReference
    {
        IStorageReference GetChild(string path);
        
        /// <summary>
        /// Uploads a byte array to firebase storage bucket.<br></br>
        /// CAUTION: Although this method returns an IStorageUploadTask, which has an AwaitAsync() method, the upload will start immediately on iOS
        /// </summary>
        IStorageTransferTask PutBytes(byte[] bytes, IStorageMetadata metaData = null);
        
        /// <summary>
        /// Uploads a file via it's file path to firebase storage bucket.<br></br>
        /// CAUTION: Although this method returns an IStorageUploadTask, which has an AwaitAsync() method, the upload will start immediately on iOS
        /// </summary>
        IStorageTransferTask PutFile(string filePath, IStorageMetadata metaData = null);
        
        /// <summary>
        /// Uploads a file via it's data stream to firebase storage bucket.<br></br>
        /// CAUTION: Although this method returns an IStorageUploadTask, which has an AwaitAsync() method, the upload will start immediately on iOS
        /// </summary>
        IStorageTransferTask PutStream(Stream stream, IStorageMetadata metaData = null);

        Task<IStorageMetadata> GetMetadataAsync();
        Task<IStorageMetadata> UpdateMetadataAsync(IStorageMetadata metadata);
        Task<string> GetDownloadUrlAsync();
        Task<IStorageListResult> ListAsync(long maxResults);
        Task<IStorageListResult> ListAllAsync();
        Task<Stream> GetStreamAsync(long maxSize);
        IStorageTransferTask DownloadFile(string destinationPath);
        Task DeleteAsync();

        IStorageReference Parent { get; }
        IStorageReference Root { get; }
        string Bucket { get; }
        string Name { get; }
        string FullPath { get; }
    }
}