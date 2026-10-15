using JetBrains.Annotations;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

internal static class ValueDocument
{
    public const string Field = "value";
}

// A document with one field, read into a property of any type, so each stored value and target type
// pairing is asserted on its own.
[Preserve(AllMembers = true)]
internal sealed class ValueDocument<T> : IFirestoreObject
{
    [FirestoreProperty(ValueDocument.Field)]
    public T? Value { get; [UsedImplicitly] private set; }
}