using JetBrains.Annotations;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed class SimpleItem : IFirestoreObject
{
    [Preserve]
    public SimpleItem()
    {
        // needed for firestore
    }

    public SimpleItem(string title)
    {
        Title = title;
    }

    public override bool Equals(object? obj)
    {
        return obj is SimpleItem other && (Id, Title).Equals((other.Id, other.Title));
    }

    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return (Id, Title).GetHashCode();
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }

    [FirestoreDocumentId]
    public string Id { get; [UsedImplicitly] private set; } = null!;

    [FirestoreProperty("title")]
    public string Title { get; [UsedImplicitly] private set; } = null!;
}