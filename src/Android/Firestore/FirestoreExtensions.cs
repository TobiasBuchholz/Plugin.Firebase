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
using NativeDocumentChange = Firebase.Firestore.DocumentChange;
using DocumentChange = Plugin.Firebase.Firestore.DocumentChange;
using FieldPath = Plugin.Firebase.Firestore.FieldPath;
using Source = Plugin.Firebase.Firestore.Source;
using NativeSource = Firebase.Firestore.Source;
using NativeFieldPath = Firebase.Firestore.FieldPath;

namespace Plugin.Firebase.Android.Firestore
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
                @this.GetType().ToAbstract(),
                @this.NewIndex,
                @this.OldIndex);
        }

        public static DocumentChangeType ToAbstract(this NativeDocumentChange.Type @this)
        {
            switch(@this.Name()) {
                case "ADDED":
                    return DocumentChangeType.Added;
                case "MODIFIED":
                    return DocumentChangeType.Modified;
                case "REMOVED":
                    return DocumentChangeType.Removed;
                default:
                    throw new FirebaseException($"Couldn't convert {@this} to abstract {nameof(DocumentChangeType)}");
            }
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
                case FieldValueType.IntegerIncrement:
                    return NativeFieldValue.Increment((long) @this.IncrementValue);
                case FieldValueType.DoubleIncrement:
                    return NativeFieldValue.Increment(@this.IncrementValue);
                case FieldValueType.Delete:
                    return NativeFieldValue.Delete();
                case FieldValueType.ServerTimestamp:
                    return NativeFieldValue.ServerTimestamp();
            }
            throw new ArgumentException($"Couldn't convert FieldValue to native because of unknown type: {@this.Type}");
        }

        public static IQuery ToAbstract(this Query @this)
        {
            return new QueryWrapper(@this);
        }

        public static ISnapshotMetadata ToAbstract(this SnapshotMetadata @this)
        {
            return new SnapshotMetadataWrapper(@this);
        }

        public static NativeSetOptions ToNative(this SetOptions options)
        {
            switch(options.Type) {
                case SetOptions.TypeMerge:
                    return NativeSetOptions.Merge();
                case SetOptions.TypeMergeFieldPaths:
                    return NativeSetOptions.MergeFieldPaths(options.FieldPaths.Select(x => NativeFieldPath.Of(x.ToArray())).ToList());
                case SetOptions.TypeMergeFields:
                    return NativeSetOptions.MergeFields(options.Fields);
                default:
                    throw new ArgumentException($"SetOptions type {options.Type} is not supported.");
            }
        }

        public static FirestoreSettings ToAbstract(this FirebaseFirestoreSettings @this)
        {
            return new FirestoreSettings(@this.Host, @this.IsPersistenceEnabled, @this.IsSslEnabled, @this.CacheSizeBytes);
        }

        public static FirebaseFirestoreSettings ToNative(this FirestoreSettings @this)
        {
            return new FirebaseFirestoreSettings.Builder()
                .SetHost(@this.Host)
                .SetPersistenceEnabled(@this.IsPersistenceEnabled)
                .SetSslEnabled(@this.IsSslEnabled)
                .SetCacheSizeBytes(@this.CacheSizeBytes)
                .Build();
        }

        public static NativeSource ToNative(this Source @this)
        {
            switch(@this) {
                case Source.Cache:
                    return NativeSource.Cache;
                case Source.Server:
                    return NativeSource.Server;
                default:
                    return NativeSource.Default;
            }
        }

        public static NativeFieldPath ToNative(this FieldPath @this)
        {
            return @this.IsDocumentId ? NativeFieldPath.DocumentId() : NativeFieldPath.Of(@this.Fields);
        }
        
        public static ICollectionReference ToAbstract(this CollectionReference @this)
        {
            return new CollectionReferenceWrapper(@this);
        }
    }
}