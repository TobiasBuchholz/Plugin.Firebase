using System;
using System.Threading.Tasks;
using Firebase.CloudFirestore;
using Foundation;
using Plugin.Firebase.Common;
using Plugin.Firebase.iOS.Extensions;
using Plugin.Firebase.iOS.Firestore;
using FBFirestore = Firebase.CloudFirestore.Firestore;

namespace Plugin.Firebase.Firestore
{
    public sealed class FirebaseFirestoreImplementation : DisposableBase, IFirebaseFirestore
    {
        private readonly FBFirestore _firestore;

        public FirebaseFirestoreImplementation()
        {
            _firestore = FBFirestore.SharedInstance;
        }

        public ICollectionReference GetCollection(string collectionPath)
        {
            return new CollectionReferenceWrapper(_firestore.GetCollection(collectionPath));
        }

        public IDocumentReference GetDocument(string documentPath)
        {
            return new DocumentReferenceWrapper(_firestore.GetDocument(documentPath));
        }

        public async Task<TResult> RunTransactionAsync<TResult>(Func<ITransaction, TResult> updateFunc)
        {
            FirebaseException exception = null;
            var result = await _firestore.RunTransactionAsync((Transaction transaction, ref NSError error) => {
                try {
                    if(error == null) {
                        return updateFunc(transaction.ToAbstract())?.ToNSObject();
                    } else {
                        exception = new FirebaseException(error.LocalizedDescription);
                    }
                } catch(Exception e) {
                    exception = new FirebaseException(e.Message);
                }
                return null;
            });
            return exception is null ? (TResult) result?.ToObject(typeof(TResult)) : throw exception;
        }

        public IWriteBatch CreateBatch()
        {
            return _firestore.CreateBatch().ToAbstract();
        }

        public void UseEmulator(string host, int port)
        {
            _firestore.UseEmulatorWithHost(host, (uint) port);
            Settings = new FirestoreSettings(Settings.Host, false, false, Settings.CacheSizeBytes);
        }

        public FirestoreSettings Settings {
            get => _firestore.Settings.ToAbstract();
            set => _firestore.Settings = value.ToNative();
        }
    }
}