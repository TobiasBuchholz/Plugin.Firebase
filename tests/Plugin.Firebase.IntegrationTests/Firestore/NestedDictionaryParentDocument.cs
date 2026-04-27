using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed class NestedDictionaryParentDocument : IFirestoreObject
    {
        [Preserve]
        public NestedDictionaryParentDocument()
        {
            // needed for firestore
        }

        public NestedDictionaryParentDocument(NestedDictionaryDocument child)
        {
            Child = child;
        }

        [FirestoreProperty("child")]
        public NestedDictionaryDocument Child { get; private set; }
    }
}
