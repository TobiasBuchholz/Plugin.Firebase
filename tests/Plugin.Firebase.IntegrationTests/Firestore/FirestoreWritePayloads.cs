using JetBrains.Annotations;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

[Preserve(AllMembers = true)]
internal sealed class SetDataPayloadDocument : IFirestoreObject
{
    [FirestoreProperty("field_a")]
    public string FieldA { get; [UsedImplicitly] private set; } = null!;

    [FirestoreProperty("field_b")]
    public string FieldB { get; [UsedImplicitly] private set; } = null!;
}

[Preserve(AllMembers = true)]
internal sealed class Issue522TransactionUpdateDocument : IFirestoreObject
{
    [FirestoreProperty("array_values")]
    public IList<string> ArrayValues { get; [UsedImplicitly] private set; } = null!;

    [FirestoreProperty("updated_at")]
    public DateTimeOffset UpdatedAt { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class BatchMergeFieldsDocument : IFirestoreObject
{
    [FirestoreProperty("untouched")]
    public string Untouched { get; [UsedImplicitly] private set; } = null!;

    [FirestoreProperty("selected")]
    public string Selected { get; [UsedImplicitly] private set; } = null!;
}

[Preserve(AllMembers = true)]
internal sealed class WriteWrapperDictionaryDocument : IFirestoreObject
{
    [FirestoreProperty("writer")]
    public string Writer { get; [UsedImplicitly] private set; } = null!;

    [FirestoreProperty("count")]
    public long Count { get; [UsedImplicitly] private set; }

    [FirestoreProperty("untouched")]
    public string Untouched { get; [UsedImplicitly] private set; } = null!;
}