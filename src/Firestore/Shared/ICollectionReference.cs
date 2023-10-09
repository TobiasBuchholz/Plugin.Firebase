namespace Plugin.Firebase.Firestore;

/// <summary>
/// A <c>ICollectionReference</c> object can be used for adding documents, getting document references, and querying for documents (using
/// the methods inherited from <c>IQuery</c>).
/// </summary>
public interface ICollectionReference : IQuery
{
    /// <summary>
    /// Gets a <c>IDocumentReference</c> object referring to the document at the specified path, relative to this collection’s own path.
    /// </summary>
    /// <param name="documentPath">The slash-separated relative path of the document for which to get a <c>IDocumentReference</c> object.</param>
    /// <returns></returns>
    IDocumentReference GetDocument(string documentPath);

    /// <summary>
    /// Returns a <c>IDocumentReference</c> object pointing to a new document with an auto-generated ID.
    /// </summary>
    IDocumentReference CreateDocument();

    /// <summary>
    /// Adds a new document to this collection with the specified data, assigning it a document ID automatically.
    /// </summary>
    /// <param name="data">An object containing the data for the new document.</param>
    Task<IDocumentReference> AddDocumentAsync(object data);
}