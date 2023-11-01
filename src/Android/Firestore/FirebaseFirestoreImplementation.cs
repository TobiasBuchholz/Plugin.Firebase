using System;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Firebase.Firestore;
using Plugin.Firebase.Android.Extensions;
using Plugin.Firebase.Common;
using Plugin.Firebase.Android.Firestore;

namespace Plugin.Firebase.Firestore
{
    public sealed class FirebaseFirestoreImplementation : DisposableBase, IFirebaseFirestore
    {
        private FirebaseFirestore _firestore;

        public FirebaseFirestoreImplementation()
        {
            _firestore = FirebaseFirestore.Instance;
        }
        
        public IQuery GetCollectionGroup(string collectionId)
        {
            return new QueryWrapper(_firestore.CollectionGroup(collectionId));
        }

        public ICollectionReference GetCollection(string collectionPath)
        {
            return new CollectionReferenceWrapper(_firestore.Collection(collectionPath));
        }

        public IDocumentReference GetDocument(string documentPath)
        {
            return new DocumentReferenceWrapper(_firestore.Document(documentPath));
        }

        public async Task<TResult> RunTransactionAsync<TResult>(Func<ITransaction, TResult> updateFunc)
        {
            var result = await _firestore.RunTransaction(new TransactionFunction<TResult>(updateFunc));
            return (TResult) result?.ToObject(typeof(TResult));
        }

        public IWriteBatch CreateBatch()
        {
            return _firestore.Batch().ToAbstract();
        }

        public Task WaitForPendingWritesAsync()
        {
            return _firestore.WaitForPendingWrites().AsAsync();
        }

        public Task DisableNetworkAsync()
        {
            return _firestore.DisableNetwork().AsAsync();
        }

        public Task EnableNetworkAsync()
        {
            return _firestore.EnableNetwork().AsAsync();
        }

        public Task ClearPersistenceAsync()
        {
            return _firestore.ClearPersistence().AsAsync();
        }

        public Task TerminateAsync()
        {
            return _firestore.Terminate().AsAsync();
        }

        public void Restart()
        {
            _firestore = FirebaseFirestore.Instance;
        }

        public void UseEmulator(string host, int port)
        {
            _firestore.UseEmulator(host, port);
        }

        public FirestoreSettings Settings {
            get => _firestore.FirestoreSettings.ToAbstract();
            set => _firestore.FirestoreSettings = value.ToNative();
        }
    }
}