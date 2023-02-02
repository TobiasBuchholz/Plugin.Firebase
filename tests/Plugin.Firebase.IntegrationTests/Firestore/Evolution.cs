using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed class Evolution : IFirestoreObject
    {
        [FirestoreProperty("pokemon_id")]
        public string PokemonId { get; private set; }

        [FirestoreProperty("name")]
        public string Name { get; private set; }
    }
}