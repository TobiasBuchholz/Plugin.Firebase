using System.Collections.Generic;
using System.Linq;
using Firebase.Storage;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.Android.Storage
{
    public sealed class StorageListResultWrapper : IStorageListResult
    {
        private readonly ListResult _wrapped;

        public StorageListResultWrapper(ListResult wrapped)
        {
            _wrapped = wrapped;
        }

        public IEnumerable<IStorageReference> Items => _wrapped.Items.Select(x => x.ToAbstract());
        public IEnumerable<IStorageReference> Prefixes => _wrapped.Prefixes.Select(x => x.ToAbstract());
        public string PageToken => _wrapped.PageToken;
    }
}