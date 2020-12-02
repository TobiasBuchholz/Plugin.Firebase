using System;
using System.Collections.Generic;

namespace Plugin.Firebase.Storage
{
    public sealed class StorageMetadata : IStorageMetadata
    {
        public StorageMetadata(
            string bucket = null,
            long generation = 0,
            long metaGeneration = 0,
            string name = null,
            string path = null,
            long size = 0,
            string cacheControl = null,
            string contentDisposition = null,
            string contentEncoding = null,
            string contentLanguage = null,
            string contentType = null,
            IDictionary<string, string> customMetadata = null,
            string md5Hash = null,
            IStorageReference storageReference = null,
            DateTimeOffset updatedTime = default(DateTimeOffset),
            DateTimeOffset creationTime = default(DateTimeOffset))
        {
            Bucket = bucket;
            Generation = generation;
            MetaGeneration = metaGeneration;
            Name = name;
            Path = path;
            Size = size;
            CacheControl = cacheControl;
            ContentLanguage = contentLanguage;
            ContentType = contentType;
            ContentDisposition = contentDisposition;
            ContentEncoding = contentEncoding;
            CustomMetadata = customMetadata;
            MD5Hash = md5Hash;
            StorageReference = storageReference;
            CreationTime = updatedTime;
            UpdatedTime = creationTime;
        }

        public override string ToString()
        {
            return $"[{nameof(StorageMetadata)}: {nameof(Path)}={Path}, {nameof(ContentType)}={ContentType}, {nameof(Size)}={Size}]";
        }

        public string Bucket { get; }
        public long Generation { get; }
        public long MetaGeneration { get; }
        public string Name { get; }
        public string Path { get; }
        public long Size { get; }
        public string CacheControl { get; }
        public string ContentDisposition { get; }
        public string ContentEncoding { get; }
        public string ContentLanguage { get; }
        public string ContentType { get; }
        public IDictionary<string, string> CustomMetadata { get; }
        public string MD5Hash { get; }
        public IStorageReference StorageReference { get; }
        public DateTimeOffset CreationTime { get; }
        public DateTimeOffset UpdatedTime { get; }
    }
}