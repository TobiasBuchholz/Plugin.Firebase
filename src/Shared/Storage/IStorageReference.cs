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
        IStorageUploadTask PutBytes(byte[] bytes);
        
        /// <summary>
        /// Uploads a file via it's file path to firebase storage bucket.<br></br>
        /// CAUTION: Although this method returns an IStorageUploadTask, which has an AwaitAsync() method, the upload will start immediately on iOS
        /// </summary>
        IStorageUploadTask PutFile(string filePath);
        
        /// <summary>
        /// Uploads a file via it's data stream to firebase storage bucket.<br></br>
        /// CAUTION: Although this method returns an IStorageUploadTask, which has an AwaitAsync() method, the upload will start immediately on iOS
        /// </summary>
        IStorageUploadTask PutStream(Stream stream);
        
        Task<string> GetDownloadUrlAsync();

        IStorageReference Parent { get; }
        IStorageReference Root { get; }
        string Bucket { get; }
        string Name { get; }
        string FullPath { get; }
    }
}