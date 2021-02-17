using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.Firestore
{
    /// <summary>
    /// A <c>IDocumentReference</c> object refers to a document location in a Firestore database and can be used to write, read, or listen to
    /// the location. The document at the referenced location may or may not exist. A <c>IDocumentReference</c> object can also be used to create
    /// a FIRCollectionReference to a subcollection.
    /// </summary>
    public interface IDocumentReference
    {
        /// <summary>
        /// Writes to the document referred to by this <c>IDocumentReference</c>. If the document does not yet exist, it will be created. If you
        /// pass <c>SetOptions</c>, the provided data can be merged into an existing document.
        /// </summary>
        /// <param name="data">The data to write to the document.</param>
        /// <param name="options">An object to configure the set behavior.</param>
        Task SetDataAsync(object data, SetOptions options = null);
        
        /// <summary>
        /// Writes to the document referred to by this <c>IDocumentReference</c>. If the document does not yet exist, it will be created. If you
        /// pass <c>SetOptions</c>, the provided data can be merged into an existing document.
        /// </summary>
        /// <param name="data">The data to write to the document.</param>
        /// <param name="options">An object to configure the set behavior.</param>
        Task SetDataAsync(Dictionary<object, object> data, SetOptions options = null);
        
        /// <summary>
        /// Writes to the document referred to by this <c>IDocumentReference</c>. If the document does not yet exist, it will be created. If you
        /// pass <c>SetOptions</c>, the provided data can be merged into an existing document.
        /// </summary>
        /// <param name="data">The data to write to the document.</param>
        Task SetDataAsync(params (object, object)[] data);
        
        /// <summary>
        /// Writes to the document referred to by this <c>IDocumentReference</c>. If the document does not yet exist, it will be created. If you
        /// pass <c>SetOptions</c>, the provided data can be merged into an existing document.
        /// </summary>
        /// <param name="options">An object to configure the set behavior.</param>
        /// <param name="data">The data to write to the document.</param>
        Task SetDataAsync(SetOptions options, params (object, object)[] data);
        
        /// <summary>
        /// Updates fields in the document referred to by this <c>IDocumentReference</c> object. If the document does not exist, the update fails.
        /// </summary>
        /// <param name="data">The data to write to the document.</param>
        Task UpdateDataAsync(Dictionary<object, object> data);
        
        /// <summary>
        /// Updates fields in the document referred to by this <c>IDocumentReference</c> object. If the document does not exist, the update fails.
        /// </summary>
        /// <param name="data">The data to write to the document.</param>
        Task UpdateDataAsync(params (string, object)[] data);
        
        /// <summary>
        /// Deletes the document referred to by this <c>IDocumentReference</c>.
        /// </summary>
        Task DeleteDocumentAsync();
        
        /// <summary>
        /// Reads the document referenced by this <c>IDocumentReference</c>. This method attempts to provide up-to-date data when possible by
        /// waiting for data from the server, but it may return cached data or fail if you are offline and the server cannot be reached.
        /// </summary>
        /// <param name="source">A value to configure the get behavior.</param>
        /// <typeparam name="T">The type of the document item.</typeparam>
        Task<IDocumentSnapshot<T>> GetDocumentSnapshotAsync<T>(Source source = Source.Default);

        /// <summary>
        /// Attaches a listener for <c>IDocumentSnapshot</c> events.
        /// </summary>
        /// <param name="onChanged">Gets invoked when the document changed.</param>
        /// <param name="onError">Gets invoked when something went wrong.</param>
        /// <param name="includeMetaDataChanges">
        /// Whether metadata-only changes (i.e. only <c>IDocumentSnapshot.Metadata</c> changed) should trigger snapshot events.
        /// </param>
        /// <typeparam name="T">The type of the document item.</typeparam>
        IDisposable AddSnapshotListener<T>(Action<IDocumentSnapshot<T>> onChanged, Action<Exception> onError = null, bool includeMetaDataChanges = false);
        
        /// <summary>
        /// The ID of the document referred to.
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// A string representing the path of the referenced document (relative to the root of the database).
        /// </summary>
        string Path { get; }
        
        /// <summary>
        /// A reference to the collection to which this <c>IDocumentReference</c> belongs.
        /// </summary>
        ICollectionReference Parent { get; }
    }
}