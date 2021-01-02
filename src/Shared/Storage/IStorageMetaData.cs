using System;
using System.Collections.Generic;

namespace Plugin.Firebase.Storage
{
    /// <summary>
    /// Class which represents the metadata on an object in Firebase Storage. This metadata is returned on successful operations, and can be
    /// used to retrieve download URLs, content types, and a storage reference to the object in question.
    /// </summary>
    public interface IStorageMetadata
    {
        /// <summary>
        /// The name of the bucket containing this object.
        /// </summary>
        string Bucket { get; }
        
        /// <summary>
        /// The content generation of this object. Used for object versioning.
        /// </summary>
        long Generation { get; }
        
        /// <summary>
        /// The version of the metadata for this object at this generation. Used for preconditions and for detecting changes in metadata.
        /// A metageneration number is only meaningful in the context of a particular generation of a particular object.
        /// </summary>
        long MetaGeneration { get; }
        
        /// <summary>
        /// The name of this object, in gs://bucket/path/to/object.txt, this is object.txt.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// The full path of this object, in gs://bucket/path/to/object.txt, this is path/to/object.txt.
        /// </summary>
        string Path { get; }
        
        /// <summary>
        /// Content-Length of the data in bytes.
        /// </summary>
        long Size { get; }
        
        /// <summary>
        /// Cache-Control directive for the object data.
        /// </summary>
        string CacheControl { get; }
        
        /// <summary>
        /// Content-Disposition of the object data.
        /// </summary>
        string ContentDisposition { get; }
        
        /// <summary>
        /// Content-Encoding of the object data.
        /// </summary>
        string ContentEncoding { get; }
        
        /// <summary>
        /// Content-Language of the object data.
        /// </summary>
        string ContentLanguage { get; }
        
        /// <summary>
        /// Content-Type of the object data.
        /// </summary>
        string ContentType { get; }
        
        /// <summary>
        /// User-provided metadata, in key/value pairs.
        /// </summary>
        IDictionary<string, string> CustomMetadata { get; }
        
        /// <summary>
        /// MD5 hash of the data; encoded using base64.
        /// </summary>
        string MD5Hash { get; }
        
        /// <summary>
        /// A reference to the object in Firebase Storage.
        /// </summary>
        IStorageReference StorageReference { get; }
        
        /// <summary>
        /// The time the <c>IStorageReference</c> was created.
        /// </summary>
        DateTimeOffset CreationTime { get; }
        
        /// <summary>
        /// The time the <c>IStorageReference</c> was last updated.
        /// </summary>
        DateTimeOffset UpdatedTime { get; }
    }
}