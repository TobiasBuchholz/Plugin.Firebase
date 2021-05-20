using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Firestore
{
    /// <summary>
    /// A <c>ICollectionReference</c> object can be used for adding documents, getting document references, and querying for documents (using
    /// the methods inherited from <c>IQuery</c>).
    /// </summary>
    public interface ICollectionReference
    {
        /// <summary>
        /// Attaches a listener for <c>IQuerySnapshot</c> events.
        /// </summary>
        /// <param name="onChanged">Gets invoked when the collection changed.</param>
        /// <param name="onError">Gets invoked when something went wrong.</param>
        /// <param name="includeMetaDataChanges">
        /// Whether metadata-only changes (i.e. only <c>IDocumentSnapshot.Metadata</c> changed) should trigger snapshot events.
        /// </param>
        /// <typeparam name="T">The type of the document item.</typeparam>
        IDisposable AddSnapshotListener<T>(Action<IQuerySnapshot<T>> onChanged, Action<Exception> onError = null, bool includeMetaDataChanges = false);
        
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
        /// Creates and returns a new <c>IQuery</c> object with the additional filter that documents must contain the specified field and the
        /// value must be equal to the specified value.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value the field must be equal to.</param>
        IQuery WhereEqualsTo(string field, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object with the additional filter that documents must contain the specified field and the
        /// value must be equal to the specified value.
        /// </summary>
        /// <param name="path">The path of the field to compare.</param>
        /// <param name="value">The value the field must be equal to.</param>
        IQuery WhereEqualsTo(FieldPath path, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object with the additional filter that documents must contain the specified field and the
        /// value must greater than the specified value.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value the field must be greater than.</param>
        IQuery WhereGreaterThan(string field, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object with the additional filter that documents must contain the specified field and the
        /// value must greater than the specified value.
        /// </summary>
        /// <param name="path">The path of the field to compare.</param>
        /// <param name="value">The value the field must be greater than.</param>
        IQuery WhereGreaterThan(FieldPath path, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object with the additional filter that documents must contain the specified field and the
        /// value must be less than the specified value.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value the field must be less than.</param>
        IQuery WhereLessThan(string field, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object with the additional filter that documents must contain the specified field and the
        /// value must be less than the specified value.
        /// </summary>
        /// <param name="path">The path of the field to compare.</param>
        /// <param name="value">The value the field must be less than.</param>
        IQuery WhereLessThan(FieldPath path, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object with the additional filter that documents must contain the specified field and the
        /// value must be greater than or equal to the specified value.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value the field must be greater than.</param>
        IQuery WhereGreaterThanOrEqualsTo(string field, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object with the additional filter that documents must contain the specified field and the
        /// value must be greater than or equal to the specified value.
        /// </summary>
        /// <param name="path">The path of the field to compare.</param>
        /// <param name="value">The value the field must be greater than.</param>
        IQuery WhereGreaterThanOrEqualsTo(FieldPath path, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object with the additional filter that documents must contain the specified field and the
        /// value must be less than or equal to the specified value.
        /// </summary>
        /// <param name="field">The name of the field to compare.</param>
        /// <param name="value">The value the field must be less than or equal to.</param>
        IQuery WhereLessThanOrEqualsTo(string field, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object with the additional filter that documents must contain the specified field and the
        /// value must be less than or equal to the specified value.
        /// </summary>
        /// <param name="path">The path of the field to compare.</param>
        /// <param name="value">The value the field must be less than or equal to.</param>
        IQuery WhereLessThanOrEqualsTo(FieldPath path, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> with the additional filter that documents must contain the specified field, it must be an array,
        /// and the array must contain the provided value.
        ///
        /// A query can have only one arrayContains filter.
        /// </summary>
        /// <param name="field">The name of the field containing an array to search</param>
        /// <param name="value">The value that must be contained in the array</param>
        IQuery WhereArrayContains(string field, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> with the additional filter that documents must contain the specified field, it must be an array,
        /// and the array must contain the provided value.
        ///
        /// A query can have only one arrayContains filter.
        /// </summary>
        /// <param name="path">The path of the field containing an array to search</param>
        /// <param name="value">The value that must be contained in the array</param>
        IQuery WhereArrayContains(FieldPath path, object value);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> with the additional filter that documents must contain the specified field, the value must be an array,
        /// and that array must contain at least one value from the provided array.
        ///
        /// A query can have only one ArrayContainsAny filter and it cannot be combined with ArrayContains or in filters.
        /// </summary>
        /// <param name="field">The name of the field containing an array to search.</param>
        /// <param name="values">The array that contains the values to match.</param>
        IQuery WhereArrayContainsAny(string field, object[] values);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> with the additional filter that documents must contain the specified field, the value must be an array,
        /// and that array must contain at least one value from the provided array.
        ///
        /// A query can have only one ArrayContainsAny filter and it cannot be combined with ArrayContains or in filters.
        /// </summary>
        /// <param name="path">The path of the field containing an array to search.</param>
        /// <param name="values">The array that contains the values to match.</param>
        IQuery WhereArrayContainsAny(FieldPath path, object[] values);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> with the additional filter that documents must contain the specified field and the value must equal
        /// one of the values from the provided array.
        ///
        /// A query can have only one in filter, and it cannot be combined with an arrayContainsAny filter.
        /// </summary>
        /// <param name="field">The name of the field to search.</param>
        /// <param name="values">The array that contains the values to match.</param>
        IQuery WhereFieldIn(string field, object[] values);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> with the additional filter that documents must contain the specified field and the value must equal
        /// one of the values from the provided array.
        ///
        /// A query can have only one in filter, and it cannot be combined with an arrayContainsAny filter.
        /// </summary>
        /// <param name="path">The name of the field to search.</param>
        /// <param name="values">The array that contains the values to match.</param>
        IQuery WhereFieldIn(FieldPath path, object[] values);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that’s additionally sorted by the specified field.
        /// </summary>
        /// <param name="field">The field to sort by.</param>
        /// <param name="descending">Whether to sort descending.</param>
        IQuery OrderBy(string field, bool descending = false);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that’s additionally sorted by the specified field.
        /// </summary>
        /// <param name="path">The path to sort by.</param>
        /// <param name="descending">Whether to sort descending.</param>
        IQuery OrderBy(FieldPath path, bool descending = false);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that starts at the provided fields relative to the order of the query. The order of
        /// the field values must match the order of the order by clauses of the query.
        /// </summary>
        /// <param name="fieldValues">The field values to start this query at, in order of the query’s order by.</param>
        IQuery StartingAt(params object[] fieldValues);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that starts at the provided document (inclusive). The starting position is relative
        /// to the order of the query. The document must contain all of the fields provided in the orderBy of this query.
        /// </summary>
        /// <param name="snapshot">The snapshot of the document to start at.</param>
        IQuery StartingAt(IDocumentSnapshot snapshot);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that starts after the provided fields relative to the order of the query. The order of
        /// the field values must match the order of the order by clauses of the query.
        /// </summary>
        /// <param name="fieldValues">The field values to start this query after, in order of the query’s order by.</param>
        IQuery StartingAfter(params object[] fieldValues);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that starts after the provided document (exclusive). The starting position is relative
        /// to the order of the query. The document must contain all of the fields provided in the orderBy of this query.
        /// </summary>
        /// <param name="snapshot">The snapshot of the document to start after.</param>
        IQuery StartingAfter(IDocumentSnapshot snapshot);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that ends at the provided fields relative to the order of the query. The order of the
        /// field values must match the order of the order by clauses of the query.
        /// </summary>
        /// <param name="fieldValues">The field values to end this query at, in order of the query’s order by.</param>
        IQuery EndingAt(params object[] fieldValues);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that ends at the provided document (exclusive). The end position is relative to the
        /// order of the query. The document must contain all of the fields provided in the orderBy of this query.
        /// </summary>
        /// <param name="snapshot">The snapshot of the document to end at.</param>
        IQuery EndingAt(IDocumentSnapshot snapshot);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that ends before the provided fields relative to the order of the query. The order of
        /// the field values must match the order of the order by clauses of the query.
        /// </summary>
        /// <param name="fieldValues">The field values to end this query before, in order of the query’s order by.</param>
        IQuery EndingBefore(params object[] fieldValues);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that ends before the provided document (exclusive). The end position is relative to
        /// the order of the query. The document must contain all of the fields provided in the orderBy of this query.
        /// </summary>
        /// <param name="snapshot">The snapshot of the document to end before.</param>
        IQuery EndingBefore(IDocumentSnapshot snapshot);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that only returns the first matching documents up to the specified number.
        /// </summary>
        /// <param name="limit">The maximum number of items to return.</param>
        IQuery LimitedTo(int limit);
        
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that only returns the last matching documents up to the specified number. A query with a
        /// <c>LimitedToLast(limit)</c> clause must have at least one orderBy clause.
        /// </summary>
        /// <param name="limit">The maximum number of items to return.</param>
        IQuery LimitedToLast(int limit);
        
        /// <summary>
        /// Adds a new document to this collection with the specified data, assigning it a document ID automatically.
        /// </summary>
        /// <param name="data">An object containing the data for the new document.</param>
        Task<IDocumentReference> AddDocumentAsync(object data);
        
        /// <summary>
        /// Returns an <c>IQuerySnapshot</c> object containing an enumerable of documents and some meta data.
        /// </summary>
        /// <param name="source">A value to configure the get behavior.</param>
        /// <typeparam name="T">The type of the document items.</typeparam>
        Task<IQuerySnapshot<T>> GetDocumentsAsync<T>(Source source = Source.Default);
    }
}