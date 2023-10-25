using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.Firestore
{
    /// <summary>
    /// A write batch is used to perform multiple writes as a single atomic unit. A <c>IWriteBatch</c> object can be acquired by calling
    /// <c>IFirebaseFirestore.CreateBatch()</c>. It provides methods for adding writes to the write batch. None of the writes will be committed
    /// (or visible locally) until <c>IWriteBatch.CommitAsync()</c> is called. Unlike transactions, write batches are persisted offline and
    /// therefore are preferable when you don’t need to condition your writes on read data.
    /// </summary>
    public interface IWriteBatch
    {
        /// <summary>
        /// Writes to the document referred to by the provided <c>IDocumentReference</c>. If the document does not yet exist, it will be created.
        /// If you pass <c>SetOptions</c>, the provided data can be merged into an existing document.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <param name="options">An object to configure the set behavior.</param>
        /// <returns>This <c>IWriteBatch</c> instance. Used for chaining method calls.</returns>
        IWriteBatch SetData(IDocumentReference document, object data, SetOptions options = null);

        /// <summary>
        /// Writes to the document referred to by the provided <c>IDocumentReference</c>. If the document does not yet exist, it will be created.
        /// If you pass <c>SetOptions</c>, the provided data can be merged into an existing document.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <param name="options">An object to configure the set behavior.</param>
        /// <returns>This <c>IWriteBatch</c> instance. Used for chaining method calls.</returns>
        IWriteBatch SetData(IDocumentReference document, Dictionary<object, object> data, SetOptions options = null);

        /// <summary>
        /// Writes to the document referred to by the provided <c>IDocumentReference</c>. If the document does not yet exist, it will be created.
        /// If you pass <c>SetOptions</c>, the provided data can be merged into an existing document.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <returns>This <c>IWriteBatch</c> instance. Used for chaining method calls.</returns>
        IWriteBatch SetData(IDocumentReference document, params (object, object)[] data);

        /// <summary>
        /// Writes to the document referred to by the provided <c>IDocumentReference</c>. If the document does not yet exist, it will be created.
        /// If you pass <c>SetOptions</c>, the provided data can be merged into an existing document.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="options">An object to configure the set behavior.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <returns>This <c>IWriteBatch</c> instance. Used for chaining method calls.</returns>
        IWriteBatch SetData(IDocumentReference document, SetOptions options, params (object, object)[] data);

        /// <summary>
        /// Updates fields in the document referred to by document. If document does not exist, the write batch will fail.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <returns>This <c>IWriteBatch</c> instance. Used for chaining method calls.</returns>
        IWriteBatch UpdateData(IDocumentReference document, Dictionary<object, object> data);

        /// <summary>
        /// Updates fields in the document referred to by document. If document does not exist, the write batch will fail.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <returns>This <c>IWriteBatch</c> instance. Used for chaining method calls.</returns>
        IWriteBatch UpdateData(IDocumentReference document, params (string, object)[] data);

        /// <summary>
        /// Deletes the document referred to by document.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to delete.</param>
        /// <returns>This <c>IWriteBatch</c> instance. Used for chaining method calls.</returns>
        IWriteBatch DeleteDocument(IDocumentReference document);

        /// <summary>
        /// Commits all of the writes in this write batch as a single atomic unit.
        /// </summary>
        Task CommitAsync();
        
        /// <summary>
        /// Commits all of the writes in this write batch as a single atomic unit. Returns immediately, without waiting for the commit to complete.
        /// </summary>
        void CommitLocal();
    }
}