using System.Collections.Generic;

namespace Plugin.Firebase.Firestore
{
    /// <summary>
    /// <c>ITransaction</c> provides methods to read and write data within a transaction.
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Reads the document referenced by document.
        /// </summary>
        /// <param name="document">A reference to the document to be read.</param>
        /// <typeparam name="T">The type of the document item.</typeparam>
        /// <returns></returns>
        IDocumentSnapshot<T> GetDocument<T>(IDocumentReference document);

        /// <summary>
        /// Writes to the document referred to by the provided <c>IDocumentReference</c> object. If the document does not yet exist, it will be
        /// created. If you pass <c>SetOptions</c>, the provided data can be merged into an existing document.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <param name="options">An object to configure the set behavior.</param>
        /// <returns>This <c>ITransaction</c> instance. Used for chaining method calls.</returns>
        ITransaction SetData(IDocumentReference document, object data, SetOptions options = null);

        /// <summary>
        /// Writes to the document referred to by the provided <c>IDocumentReference</c> object. If the document does not yet exist, it will be
        /// created. If you pass <c>SetOptions</c>, the provided data can be merged into an existing document.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <param name="options">An object to configure the set behavior.</param>
        /// <returns>This <c>ITransaction</c> instance. Used for chaining method calls.</returns>
        ITransaction SetData(IDocumentReference document, Dictionary<object, object> data, SetOptions options = null);

        /// <summary>
        /// Writes to the document referred to by the provided <c>IDocumentReference</c> object. If the document does not yet exist, it will be
        /// created.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <returns>This <c>ITransaction</c> instance. Used for chaining method calls.</returns>
        ITransaction SetData(IDocumentReference document, params (object, object)[] data);

        /// <summary>
        /// Writes to the document referred to by the provided <c>IDocumentReference</c> object. If the document does not yet exist, it will be
        /// created. If you pass <c>SetOptions</c>, the provided data can be merged into an existing document.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="options">An object to configure the set behavior.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <returns>This <c>ITransaction</c> instance. Used for chaining method calls.</returns>
        ITransaction SetData(IDocumentReference document, SetOptions options, params (object, object)[] data);

        /// <summary>
        /// Updates fields in the document referred to by document. If the document does not exist, the transaction will fail.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <returns>This <c>ITransaction</c> instance. Used for chaining method calls.</returns>
        ITransaction UpdateData(IDocumentReference document, Dictionary<object, object> data);

        /// <summary>
        /// Updates fields in the document referred to by document. If the document does not exist, the transaction will fail.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to overwrite.</param>
        /// <param name="data">The data to write to the document.</param>
        /// <returns>This <c>ITransaction</c> instance. Used for chaining method calls.</returns>
        ITransaction UpdateData(IDocumentReference document, params (string, object)[] data);

        /// <summary>
        /// Deletes the document referred to by document.
        /// </summary>
        /// <param name="document">The <c>IDocumentReference</c> to delete.</param>
        /// <returns>This <c>ITransaction</c> instance. Used for chaining method calls.</returns>
        ITransaction DeleteDocument(IDocumentReference document);
    }
}