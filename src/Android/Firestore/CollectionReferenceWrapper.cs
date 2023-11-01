using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Firebase.Firestore;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.Android.Firestore
{
    public sealed class CollectionReferenceWrapper : QueryWrapper, ICollectionReference
    {
        private readonly CollectionReference _wrapped;

        public CollectionReferenceWrapper(CollectionReference reference)
            : base(reference)
        {
            _wrapped = reference;
        }

        public IDocumentReference GetDocument(string documentPath)
        {
            return new DocumentReferenceWrapper(_wrapped.Document(documentPath));
        }

        public IDocumentReference CreateDocument()
        {
            return new DocumentReferenceWrapper(_wrapped.Document());
        }

        public async Task<IDocumentReference> AddDocumentAsync(object data)
        {
            var documentReference = (DocumentReference) await _wrapped.Add(data.ToHashMap());
            return new DocumentReferenceWrapper(documentReference);
        }
    }
}