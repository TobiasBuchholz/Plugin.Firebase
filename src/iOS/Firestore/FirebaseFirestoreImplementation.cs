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
            var result = await _firestore.RunTransactionAsync((Transaction transaction, ref NSError error) => {
                if(error == null) {
                    return updateFunc(transaction.ToAbstract()).ToNSObject();
                } else {
                    throw new FirebaseException(error.LocalizedDescription);
                }
            });
            return (TResult) result?.ToObject(typeof(TResult));
        }

        public IWriteBatch CreateBatch()
        {
            return _firestore.CreateBatch().ToAbstract();
        }
    }
}