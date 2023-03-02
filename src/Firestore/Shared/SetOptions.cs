namespace Plugin.Firebase.Firestore;

/// <summary>
/// An options object that configures the behavior of <c>Set()</c> calls. By providing one of the <c>SetOptions</c> objects returned by
/// <c>Merge()</c>, <c>MergeFields(fields)</c> and <c>MergeFieldPaths(fieldPaths)</c>, the <c>Set()</c> calls in <c>IDocumentReference</c>,
/// <c>WriteBatch</c> and <c>Transaction</c> can be configured to perform granular merges instead of overwriting the target documents
/// in their entirety.
/// </summary>
public sealed class SetOptions
{
    /// <summary>
    /// Changes the behavior of <c>Set()</c> calls to only replace the values specified in its data argument. Fields omitted from the
    /// <c>Set()</c> call will remain untouched.
    /// </summary>
    public static SetOptions Merge() => new SetOptions(TypeMerge);

    /// <summary>
    /// Changes the behavior of <c>Set()</c> calls to only replace the given fields. Any field that is not specified in fields is ignored
    /// and remains untouched. It is an error to pass a <c>SetOptions</c> object to a <c>Set()</c> call that is missing a value for any of
    /// the fields specified here in its to data argument.
    /// </summary>
    /// <param name="fieldPaths">The list of fields to merge.</param>
    public static SetOptions MergeFieldPaths(IList<IList<string>> fieldPaths) => new SetOptions(TypeMergeFieldPaths, fieldPaths);

    /// <summary>
    /// Changes the behavior of <c>Set()</c> calls to only replace the given fields. Any field that is not specified in fields is ignored
    /// and remains untouched. It is an error to pass a SetOptions object to a <c>Set()</c> call that is missing a value for any of the
    /// fields specified here.
    /// </summary>
    /// <param name="fields">The list of fields to merge. Fields can contain dots to reference nested fields within the document.</param>
    public static SetOptions MergeFields(params string[] fields) => new SetOptions(TypeMergeFields, fields: fields);

    /// <summary>
    /// Changes the behavior of <c>Set()</c> calls to only replace the given fields. Any field that is not specified in fields is ignored
    /// and remains untouched. It is an error to pass a SetOptions object to a <c>Set()</c> call that is missing a value for any of the
    /// fields specified here.
    /// </summary>
    /// <param name="fields">The list of fields to merge. Fields can contain dots to reference nested fields within the document.</param>
    public static SetOptions MergeFields(IList<string> fields) => new SetOptions(TypeMergeFields, fields: fields);

    public const int TypeMerge = 0;
    public const int TypeMergeFieldPaths = 1;
    public const int TypeMergeFields = 2;

    public SetOptions(int type, IList<IList<string>> fieldPaths = null, IList<string> fields = null)
    {
        Type = type;
        FieldPaths = fieldPaths;
        Fields = fields;
    }

    /// <summary>
    /// The type of the merge as integer (TypeMerge = 0, TypeMergeFieldPaths = 1, TypeMergeFields = 2)
    /// </summary>
    public int Type { get; }

    /// <summary>
    /// The list of fields to merge.
    /// </summary>
    public IList<IList<string>> FieldPaths { get; }

    /// <summary>
    /// The list of fields to merge.
    /// </summary>
    public IList<string> Fields { get; }
}