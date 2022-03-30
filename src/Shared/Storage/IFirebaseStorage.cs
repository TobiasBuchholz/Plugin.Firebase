using System;

namespace Plugin.Firebase.Storage
{
    /// <summary>
    /// FirebaseStorage is a service that supports uploading and downloading binary objects, such as images, videos, and other files to
    /// Google Cloud Storage.
    /// </summary>
    public interface IFirebaseStorage : IDisposable
    {
        /// <summary>
        /// Creates a new <c>IStorageReference</c> initialized at the root Firebase Storage location. 
        /// </summary>
        /// <returns></returns>
        IStorageReference GetRootReference();

        /// <summary>
        /// Creates a <c>IStorageReference</c> given a gs:// or // URL pointing to a Firebase Storage location.
        /// </summary>
        /// <param name="url">
        /// A gs:// or http[s]:// URL used to initialize the reference. For example, you can pass in a download URL retrieved from
        /// GetDownloadUrlAsync(). An error is thrown if the url is not associated with the FirebaseApp used to initialize this FirebaseStorage.
        /// </param>
        IStorageReference GetReferenceFromUrl(string url);

        /// <summary>
        /// Creates a new <c>IStorageReference</c> initialized with a child Firebase Storage location.
        /// </summary>
        /// <param name="path">A relative path from the root to initialize the reference with, for instance "path/to/object"</param>
        /// <returns></returns>
        IStorageReference GetReferenceFromPath(string path);
    }
}