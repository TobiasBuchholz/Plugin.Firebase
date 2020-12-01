using System.Collections.Generic;

namespace Plugin.Firebase.Storage
{
    public interface IStorageListResult
    {
        IEnumerable<IStorageReference> Items { get; }
        IEnumerable<IStorageReference> Prefixes { get; }
        string PageToken { get; }
    }
}