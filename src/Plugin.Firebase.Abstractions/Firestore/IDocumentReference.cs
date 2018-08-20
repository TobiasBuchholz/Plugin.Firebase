using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.Abstractions.Firestore
{
    public interface IDocumentReference
    {
        Task SetDataAsync(object data, SetOptions options = null);
        Task SetDataAsync(Dictionary<object, object> data, SetOptions options = null);
        Task SetDataAsync(params (object, object)[] data);
        Task SetDataAsync(SetOptions options, params (object, object)[] data);
        Task UpdateDataAsync(Dictionary<object, object> data);
        Task DeleteDocumentAsync();
        
        string Id { get; }
        string Path { get; }
        ICollectionReference Parent { get; }
    }
}