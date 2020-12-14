using System;
using System.Linq;
using Firebase;
using Firebase.Firestore;
using Plugin.Firebase.Android.Extensions;
using Plugin.Firebase.Firestore;
using FieldValue = Plugin.Firebase.Firestore.FieldValue;
using NativeFieldValue = Firebase.Firestore.FieldValue;
using NativeSetOptions = Firebase.Firestore.SetOptions;
using SetOptions = Plugin.Firebase.Firestore.SetOptions;

namespace Plugin.Firebase.Android.Firestore
{
    public static class FirestoreExtensions
    {
        public static IDocumentSnapshot<T> ToAbstract<T>(this DocumentSnapshot @this)
        {
            return new DocumentSnapshotWrapper<T>(@this);
        }
        
        public static DocumentReference ToNative(this IDocumentReference @this)
        {
            if(@this is DocumentReferenceWrapper wrapper) {
                return wrapper.Wrapped;
            }
            throw new FirebaseException($"This implementation of {nameof(IDocumentReference)} is not supported for this method");
        }
        
        public static ITransaction ToAbstract(this Transaction @this)
        {
            return new TransactionWrapper(@this);
        }
        
        public static IWriteBatch ToAbstract(this WriteBatch @this)
        {
            return new WriteBatchWrapper(@this);
        }
        
        public static NativeFieldValue ToNative(this FieldValue @this)
        {
            switch(@this.Type) {
                case FieldValueType.ArrayUnion:
                    return NativeFieldValue.ArrayUnion(@this.Elements.Select(x => x.ToJavaObject()).ToArray());
                case FieldValueType.ArrayRemove:
                    return NativeFieldValue.ArrayRemove(@this.Elements.Select(x => x.ToJavaObject()).ToArray());
            }
            throw new ArgumentException($"Couldn't convert FieldValue to native because of unknown type: {@this.Type}");
        }
        
        public static NativeSetOptions ToNative(this SetOptions options)
        {
            switch(options.Type) {
                case SetOptions.TypeMerge:
                    return NativeSetOptions.Merge();
                case SetOptions.TypeMergeFieldPaths:
                    return NativeSetOptions.MergeFieldPaths(options.FieldPaths.Select(x => FieldPath.Of(x.ToArray())).ToList());
                case SetOptions.TypeMergeFields:
                    return NativeSetOptions.MergeFields(options.Fields);
                default:
                    throw new ArgumentException($"SetOptions type {options.Type} is not supported.");
            }
        }
    }
}