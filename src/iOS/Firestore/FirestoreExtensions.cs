using System;
using System.Linq;
using Firebase.CloudFirestore;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.iOS.Extensions;
using DocumentChange = Plugin.Firebase.Firestore.DocumentChange;
using DocumentChangeType = Plugin.Firebase.Firestore.DocumentChangeType;
using FieldValue = Plugin.Firebase.Firestore.FieldValue;
using NativeFieldValue = Firebase.CloudFirestore.FieldValue;
using NativeDocumentChange = Firebase.CloudFirestore.DocumentChange;
using NativeDocumentChangeType = Firebase.CloudFirestore.DocumentChangeType;

namespace Plugin.Firebase.iOS.Firestore
{
    public static class FirestoreExtensions
    {
        public static IDocumentSnapshot ToAbstract(this DocumentSnapshot @this)
        {
            return new DocumentSnapshotWrapper(@this);
        }
        
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

        public static IDocumentReference ToAbstract(this DocumentReference @this)
        {
            return new DocumentReferenceWrapper(@this);
        }
        
        public static DocumentSnapshot ToNative(this IDocumentSnapshot @this)
        {
            if(@this is DocumentSnapshotWrapper wrapper) {
                return wrapper.Wrapped;
            }
            throw new FirebaseException($"This implementation of {nameof(IDocumentSnapshot)} is not supported for this method");
        }

        public static DocumentChange ToAbstract(this NativeDocumentChange @this)
        {
            return new DocumentChange(
                @this.Document.ToAbstract(),
                @this.Type.ToAbstract(), 
                (int) @this.NewIndex,
                (int) @this.OldIndex);
        }

        public static DocumentChangeType ToAbstract(this NativeDocumentChangeType @this)
        {
            switch(@this) {
                case NativeDocumentChangeType.Added:
                    return DocumentChangeType.Added;
                case NativeDocumentChangeType.Modified:
                    return DocumentChangeType.Modified;
                case NativeDocumentChangeType.Removed:
                    return DocumentChangeType.Removed;
                default:
                    throw new FirebaseException($"Couldn't convert {@this} to abstract {nameof(DocumentChangeType)}");
            }
        }

        public static IQuery ToAbstract(this Query @this)
        {
            return new QueryWrapper(@this);
        }

        public static ISnapshotMetadata ToAbstract(this SnapshotMetadata @this)
        {
            return new SnapshotMetadataWrapper(@this);
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
                    return NativeFieldValue.FromArrayUnion(@this.Elements.Select(x => x.ToNSObject()).ToArray());
                case FieldValueType.ArrayRemove:
                    return NativeFieldValue.FromArrayRemove(@this.Elements.Select(x => x.ToNSObject()).ToArray());
                case FieldValueType.IntegerIncrement:
                    return NativeFieldValue.FromIntegerIncrement((long) @this.IncrementValue);
                case FieldValueType.DoubleIncrement:
                    return NativeFieldValue.FromDoubleIncrement(@this.IncrementValue);
                case FieldValueType.Delete:
                    return NativeFieldValue.Delete;
                case FieldValueType.ServerTimestamp:
                    return NativeFieldValue.ServerTimestamp;
            }
            throw new ArgumentException($"Couldn't convert FieldValue to native because of unknown type: {@this.Type}");
        }
    }
}