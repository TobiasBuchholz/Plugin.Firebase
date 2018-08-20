using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.CloudFirestore;
using Plugin.Firebase.Abstractions.Firestore;

namespace Plugin.Firebase.Firestore
{
    public sealed class DocumentReferenceWrapper : IDocumentReference
    {
        private readonly DocumentReference _reference;
        
        public DocumentReferenceWrapper(DocumentReference reference)
        {
            _reference = reference;
        }

        public Task SetDataAsync(object data, SetOptions options = null)
        {
            return SetDataAsync(data.ToDictionary<object>(), options);
        }

        public Task SetDataAsync(Dictionary<object, object> data, SetOptions options)
        {
            if(options == null) {
                return _reference.SetDataAsync(data);
            }

            switch(options.Type) {
                case SetOptions.TypeMerge:
                    return _reference.SetDataAsync(data, true);
                case SetOptions.TypeMergeFieldPaths:
                    return _reference.SetDataAsync(data, options.FieldPaths.Select(x => new FieldPath(x.ToArray())).ToArray());
                case SetOptions.TypeMergeFields:
                    return _reference.SetDataAsync(data, options.Fields.ToArray());
                default:
                    throw new ArgumentException($"SetOptions type {options.Type} is not supported.");
            }
        }

        public Task SetDataAsync(params (object, object)[] data)
        {
            return SetDataAsync(data.ToDictionary());
        }

        public Task SetDataAsync(SetOptions options, params (object, object)[] data)
        {
            return SetDataAsync(data.ToDictionary(), options);
        }

        public Task UpdateDataAsync(Dictionary<object, object> data)
        {
            return _reference.UpdateDataAsync(data);
        }

        public Task DeleteDocumentAsync()
        {
            return _reference.DeleteDocumentAsync();
        }

        public string Id => _reference.Id;
        public string Path => _reference.Path;
        public ICollectionReference Parent => new CollectionReferenceWrapper(_reference.Parent);
    }
}