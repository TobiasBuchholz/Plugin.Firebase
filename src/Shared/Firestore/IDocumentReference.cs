using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.Firestore
{
    public interface IDocumentReference
    {
        Task SetDataAsync(object data, SetOptions options = null);
        Task SetDataAsync(Dictionary<object, object> data, SetOptions options = null);
        Task SetDataAsync(params (object, object)[] data);
        Task SetDataAsync(SetOptions options, params (object, object)[] data);
        Task UpdateDataAsync(Dictionary<object, object> data);
        Task UpdateDataAsync(params (string, object)[] data);
        Task DeleteDocumentAsync();
        Task<IDocumentSnapshot<T>> GetDocumentSnapshotAsync<T>();

        IDisposable AddSnapshotListener<T>(Action<IDocumentSnapshot<T>> onChanged, Action<Exception> onError = null, bool includeMetaDataChanges = false);
        
        string Id { get; }
        string Path { get; }
        ICollectionReference Parent { get; }
    }
}