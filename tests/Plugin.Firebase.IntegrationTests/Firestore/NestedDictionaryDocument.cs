using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed class NestedDictionaryDocument : IFirestoreObject
    {
        [Preserve]
        public NestedDictionaryDocument()
        {
            // needed for firestore
        }

        public NestedDictionaryDocument(Dictionary<string, Dictionary<string, int>> foo)
        {
            Foo = foo;
        }

        [FirestoreProperty("foo")]
        public Dictionary<string, Dictionary<string, int>> Foo { get; private set; }
    }
}