using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Firebase.Firestore;
using Plugin.Firebase.Abstractions.Firestore;

namespace Plugin.Firebase.Firestore
{
    public sealed class CollectionReferenceWrapper : ICollectionReference
    {
        private readonly CollectionReference _reference;
        
        public CollectionReferenceWrapper(CollectionReference reference)
        {
            _reference = reference;
        }

        public IDocumentReference GetDocument(string documentPath)
        {
            return new DocumentReferenceWrapper(_reference.Document(documentPath));
        }

        public IDocumentReference CreateDocument()
        {
            return new DocumentReferenceWrapper(_reference.Document());
        }

        public async Task<IDocumentReference> AddDocumentAsync(object data)
        {
            var documentReference = (DocumentReference) await _reference.Add(data.ToDictionary<object>().ToHashMap());
            return new DocumentReferenceWrapper(documentReference);
        }
    }
}