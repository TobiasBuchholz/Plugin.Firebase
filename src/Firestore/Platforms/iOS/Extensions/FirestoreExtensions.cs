using Firebase.CloudFirestore;
using Plugin.Firebase.Core.Exceptions;
using NativeDocumentChange = Firebase.CloudFirestore.DocumentChange;
using NativeDocumentChangeType = Firebase.CloudFirestore.DocumentChangeType;
using NativeFieldPath = Firebase.CloudFirestore.FieldPath;
using NativeFieldValue = Firebase.CloudFirestore.FieldValue;
using NativeFirestoreSettings = Firebase.CloudFirestore.FirestoreSettings;
using NativeSource = Firebase.CloudFirestore.FirestoreSource;

namespace Plugin.Firebase.Firestore.Platforms.iOS.Extensions
{
    /// <summary>
    /// Extension methods for converting between native iOS Firestore types and abstract wrapper types.
    /// </summary>
    public static class FirestoreExtensions
    {
        /// <summary>
        /// Converts a native iOS document snapshot to an abstract wrapper.
        /// </summary>
        /// <param name="this">The native document snapshot.</param>
        /// <returns>An abstract document snapshot wrapper.</returns>
        public static IDocumentSnapshot ToAbstract(this DocumentSnapshot @this)
        {
            return new DocumentSnapshotWrapper(@this);
        }

        /// <summary>
        /// Converts a native iOS document snapshot to a typed abstract wrapper.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the document data into.</typeparam>
        /// <param name="this">The native document snapshot.</param>
        /// <returns>A typed abstract document snapshot wrapper.</returns>
        public static IDocumentSnapshot<T> ToAbstract<T>(this DocumentSnapshot @this)
        {
            return new DocumentSnapshotWrapper<T>(@this);
        }

        /// <summary>
        /// Converts an abstract document reference to the native iOS type.
        /// </summary>
        /// <param name="this">The abstract document reference.</param>
        /// <returns>The native iOS document reference.</returns>
        /// <exception cref="FirebaseException">Thrown if the implementation is not supported.</exception>
        public static DocumentReference ToNative(this IDocumentReference @this)
        {
            if(@this is DocumentReferenceWrapper wrapper) {
                return wrapper.Wrapped;
            }
            throw new FirebaseException(
                $"This implementation of {nameof(IDocumentReference)} is not supported for this method"
            );
        }

        /// <summary>
        /// Converts a native iOS document reference to an abstract wrapper.
        /// </summary>
        /// <param name="this">The native document reference.</param>
        /// <returns>An abstract document reference wrapper.</returns>
        public static IDocumentReference ToAbstract(this DocumentReference @this)
        {
            return new DocumentReferenceWrapper(@this);
        }

        /// <summary>
        /// Converts an abstract document snapshot to the native iOS type.
        /// </summary>
        /// <param name="this">The abstract document snapshot.</param>
        /// <returns>The native iOS document snapshot.</returns>
        /// <exception cref="FirebaseException">Thrown if the implementation is not supported.</exception>
        public static DocumentSnapshot ToNative(this IDocumentSnapshot @this)
        {
            if(@this is DocumentSnapshotWrapper wrapper) {
                return wrapper.Wrapped;
            }
            throw new FirebaseException(
                $"This implementation of {nameof(IDocumentSnapshot)} is not supported for this method"
            );
        }

        /// <summary>
        /// Converts a native iOS document change to an abstract typed document change.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the document data into.</typeparam>
        /// <param name="this">The native document change.</param>
        /// <returns>A typed abstract document change.</returns>
        public static DocumentChange<T> ToAbstract<T>(this NativeDocumentChange @this)
        {
            return new DocumentChange<T>(
                @this.Document.ToAbstract<T>(),
                @this.Type.ToAbstract(),
                (int) @this.NewIndex,
                (int) @this.OldIndex
            );
        }

        /// <summary>
        /// Converts a native iOS document change type to the abstract enum.
        /// </summary>
        /// <param name="this">The native document change type.</param>
        /// <returns>The abstract document change type.</returns>
        /// <exception cref="FirebaseException">Thrown if the type is unknown.</exception>
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
                    throw new FirebaseException(
                        $"Couldn't convert {@this} to abstract {nameof(DocumentChangeType)}"
                    );
            }
        }

        /// <summary>
        /// Converts a native iOS query to an abstract wrapper.
        /// </summary>
        /// <param name="this">The native query.</param>
        /// <returns>An abstract query wrapper.</returns>
        public static IQuery ToAbstract(this Query @this)
        {
            return new QueryWrapper(@this);
        }

        /// <summary>
        /// Converts native iOS snapshot metadata to an abstract wrapper.
        /// </summary>
        /// <param name="this">The native snapshot metadata.</param>
        /// <returns>An abstract snapshot metadata wrapper.</returns>
        public static ISnapshotMetadata ToAbstract(this SnapshotMetadata @this)
        {
            return new SnapshotMetadataWrapper(@this);
        }

        /// <summary>
        /// Converts a native iOS transaction to an abstract wrapper.
        /// </summary>
        /// <param name="this">The native transaction.</param>
        /// <returns>An abstract transaction wrapper.</returns>
        public static ITransaction ToAbstract(this Transaction @this)
        {
            return new TransactionWrapper(@this);
        }

        /// <summary>
        /// Converts a native iOS write batch to an abstract wrapper.
        /// </summary>
        /// <param name="this">The native write batch.</param>
        /// <returns>An abstract write batch wrapper.</returns>
        public static IWriteBatch ToAbstract(this WriteBatch @this)
        {
            return new WriteBatchWrapper(@this);
        }

        /// <summary>
        /// Converts an abstract field value to the native iOS type.
        /// </summary>
        /// <param name="this">The abstract field value.</param>
        /// <returns>The native iOS field value.</returns>
        /// <exception cref="ArgumentException">Thrown if the field value type is unknown.</exception>
        public static NativeFieldValue ToNative(this FieldValue @this)
        {
            switch(@this.Type) {
                case FieldValueType.ArrayUnion:
                    return NativeFieldValue.FromArrayUnion(
                        @this.Elements.Select(x => x.ToNSObject()).ToArray()
                    );
                case FieldValueType.ArrayRemove:
                    return NativeFieldValue.FromArrayRemove(
                        @this.Elements.Select(x => x.ToNSObject()).ToArray()
                    );
                case FieldValueType.IntegerIncrement:
                    return NativeFieldValue.FromIntegerIncrement((long) @this.IncrementValue);
                case FieldValueType.DoubleIncrement:
                    return NativeFieldValue.FromDoubleIncrement(@this.IncrementValue);
                case FieldValueType.Delete:
                    return NativeFieldValue.Delete;
                case FieldValueType.ServerTimestamp:
                    return NativeFieldValue.ServerTimestamp;
            }
            throw new ArgumentException(
                $"Couldn't convert FieldValue to native because of unknown type: {@this.Type}"
            );
        }

        /// <summary>
        /// Converts native iOS Firestore settings to abstract settings.
        /// </summary>
        /// <param name="this">The native Firestore settings.</param>
        /// <returns>Abstract Firestore settings.</returns>
        public static FirestoreSettings ToAbstract(this NativeFirestoreSettings @this)
        {
            return new FirestoreSettings(@this.Host, @this.SslEnabled);
        }

        /// <summary>
        /// Converts abstract Firestore settings to the native iOS type.
        /// </summary>
        /// <param name="this">The abstract Firestore settings.</param>
        /// <returns>Native iOS Firestore settings.</returns>
        public static NativeFirestoreSettings ToNative(this FirestoreSettings @this)
        {
            return new NativeFirestoreSettings {
                Host = @this.Host,
                SslEnabled = @this.IsSslEnabled,
            };
        }

        /// <summary>
        /// Converts an abstract data source to the native iOS type.
        /// </summary>
        /// <param name="this">The abstract source.</param>
        /// <returns>The native iOS Firestore source.</returns>
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

        /// <summary>
        /// Converts an abstract field path to the native iOS type.
        /// </summary>
        /// <param name="this">The abstract field path.</param>
        /// <returns>The native iOS field path.</returns>
        public static NativeFieldPath ToNative(this FieldPath @this)
        {
            return @this.IsDocumentId
                ? NativeFieldPath.GetDocumentId()
                : new NativeFieldPath(@this.Fields);
        }

        /// <summary>
        /// Converts a native iOS collection reference to an abstract wrapper.
        /// </summary>
        /// <param name="this">The native collection reference.</param>
        /// <returns>An abstract collection reference wrapper.</returns>
        public static ICollectionReference ToAbstract(this CollectionReference @this)
        {
            return new CollectionReferenceWrapper(@this);
        }
    }
}