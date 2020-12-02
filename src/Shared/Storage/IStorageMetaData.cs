using System;
using System.Collections.Generic;

namespace Plugin.Firebase.Storage
{
    public interface IStorageMetadata
    {
        string Bucket { get; }
        long Generation { get; }
        long MetaGeneration { get; }
        string Name { get; }
        string Path { get; }
        long Size { get; }
        string CacheControl { get; }
        string ContentDisposition { get; }
        string ContentEncoding { get; }
        string ContentLanguage { get; }
        string ContentType { get; }
        IDictionary<string, string> CustomMetadata { get; }
        string MD5Hash { get; }
        IStorageReference StorageReference { get; }
        DateTimeOffset CreationTime { get; }
        DateTimeOffset UpdatedTime { get; }
    }
}