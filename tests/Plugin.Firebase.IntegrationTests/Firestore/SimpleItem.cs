using Android.Runtime;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
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

        public override bool Equals(object obj)
        {
            if(obj is SimpleItem other) {
                return (Id, Title).Equals((Id, Title));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (Id, Title).GetHashCode();
        }

        [FirestoreDocumentId]
        public string Id { get; private set; }

        [FirestoreProperty("title")]
        public string Title { get; private set; }
    }
}