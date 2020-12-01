using System.Collections.Generic;
using System.Linq;
using Firebase.Storage;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.iOS.Storage
{
    public sealed class StorageListResultsWrapper : IStorageListResult
    {
        private readonly StorageListResult _wrapped;

        public StorageListResultsWrapper(StorageListResult storageListResult)
        {
            _wrapped = storageListResult;
        }

        public IEnumerable<IStorageReference> Items => _wrapped.Items.Select(x => new StorageReferenceWrapper(x));
        public IEnumerable<IStorageReference> Prefixes => _wrapped.Prefixes.Select(x => new StorageReferenceWrapper(x));
        public string PageToken => _wrapped.PageToken;
    }
}