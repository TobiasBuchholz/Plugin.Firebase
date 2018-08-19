using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Abstractions.Firestore
{
    public interface IFirebaseFirestore : IDisposable
    {
        ICollectionReference GetCollection(string collectionPath);
    }
}