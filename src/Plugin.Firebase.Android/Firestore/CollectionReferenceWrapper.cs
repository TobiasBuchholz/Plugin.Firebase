using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Firebase.Firestore;
using Plugin.Firebase.Abstractions.Firestore;
using Plugin.Firebase.Android.Extensions;

namespace Plugin.Firebase.Android.Firestore
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

        public IQuery WhereEqualsTo(string field, object value)
        {
            return new QueryWrapper(_reference.WhereEqualTo(field, value.ToJavaObject()));
        }

        public IQuery WhereGreaterThan(string field, object value)
        {
            return new QueryWrapper(_reference.WhereGreaterThan(field, value.ToJavaObject()));
        }

        public IQuery WhereLessThan(string field, object value)
        {
            return new QueryWrapper(_reference.WhereLessThan(field, value.ToJavaObject()));
        }

        public IQuery WhereGreaterThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_reference.WhereGreaterThanOrEqualTo(field, value.ToJavaObject()));
        }

        public IQuery WhereLessThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_reference.WhereLessThanOrEqualTo(field, value.ToJavaObject()));
        }

        public IQuery OrderBy(string field)
        {
            return new QueryWrapper(_reference.OrderBy(field));
        }

        public IQuery StartingAt(object[] fieldValues)
        {
            return new QueryWrapper(_reference.StartAt(fieldValues.Select(x => x.ToJavaObject()).ToArray()));
        }

        public IQuery EndingAt(object[] fieldValues)
        {
            return new QueryWrapper(_reference.EndAt(fieldValues.Select(x => x.ToJavaObject()).ToArray()));
        }

        public async Task<IDocumentReference> AddDocumentAsync(object data)
        {
            var documentReference = (DocumentReference) await _reference.Add(data.ToHashMap());
            return new DocumentReferenceWrapper(documentReference);
        }
    }
}