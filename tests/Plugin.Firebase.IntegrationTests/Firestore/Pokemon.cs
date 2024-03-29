using Plugin.Firebase.Core.Extensions;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed class Pokemon : IFirestoreObject
    {
        [Preserve]
        public Pokemon()
        {
            // needed for firestore
        }

        public Pokemon(
            string id = null,
            string name = null,
            double weightInKg = 0,
            float heightInCm = 0,
            long sightingCount = 0,
            bool isFromFirstGeneration = false,
            PokeType pokeType = default,
            IList<string> moves = null,
            IList<double> someNumbers = null,
            SightingLocation firstSightingLocation = null,
            IList<SimpleItem> items = null,
            IDocumentReference originalReference = null)
        {
            Id = id;
            Name = name;
            WeightInKg = weightInKg;
            HeightInCm = heightInCm;
            SightingCount = sightingCount;
            IsFromFirstGeneration = isFromFirstGeneration;
            PokeType = pokeType;
            Moves = moves;
            SomeNumbers = someNumbers;
            FirstSightingLocation = firstSightingLocation;
            Items = items;
            CreationDate = DateTime.Now;
            OriginalReference = originalReference;
        }

        /// <summary>
        /// Get a clone from the current pokemon
        /// </summary>
        /// <param name="originalReference">Reference to the original document that shall be cloned</param>
        /// <returns>A copy of the current pokemon</returns>
        public Pokemon Clone(IDocumentReference originalReference)
        {
            return new Pokemon(
                id: $"{this.Id}_copied",
                name: this.Name,
                weightInKg: this.WeightInKg,
                heightInCm: this.HeightInCm,
                isFromFirstGeneration: this.IsFromFirstGeneration,
                pokeType: this.PokeType,
                moves: this.Moves.ToList(),
                someNumbers: this.SomeNumbers.ToList(),
                firstSightingLocation: this.FirstSightingLocation,
                items: this.Items.ToList(),
                originalReference: originalReference);
        }

        public override bool Equals(object obj)
        {
            if(obj is Pokemon other) {
                return (Id, Name, WeightInKg, HeightInCm, SightingCount, IsFromFirstGeneration, PokeType, FirstSightingLocation)
                    .Equals((other.Id, other.Name, other.WeightInKg, other.HeightInCm, other.SightingCount, other.IsFromFirstGeneration, other.PokeType, other.FirstSightingLocation)) &&
                    Moves.SequenceEqualSafe(other.Moves) &&
                    SomeNumbers.SequenceEqualSafe(other.SomeNumbers) &&
                    Items.SequenceEqualSafe(other.Items);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (Id, Name, WeightInKg, HeightInCm, SightingCount, IsFromFirstGeneration, PokeType, Moves, SomeNumbers, FirstSightingLocation, Items, CreationDate).GetHashCode();
        }

        public override string ToString()
        {
            return $"[{nameof(Pokemon)}: {nameof(Id)}={Id}, {nameof(Name)}={Name}]";
        }

        [FirestoreDocumentId]
        public string Id { get; private set; }

        [FirestoreProperty("name")]
        public string Name { get; private set; }

        [FirestoreProperty("weight_in_kg")]
        public double WeightInKg { get; private set; }

        [FirestoreProperty("height_in_cm")]
        public float HeightInCm { get; private set; }

        [FirestoreProperty("sighting_count")]
        public long SightingCount { get; private set; }

        [FirestoreProperty("is_from_first_generation")]
        public bool IsFromFirstGeneration { get; private set; }

        [FirestoreProperty("poke_type")]
        public PokeType PokeType { get; private set; }

        [FirestoreProperty("moves")]
        public IList<string> Moves { get; private set; }

        [FirestoreProperty("some_numbers")]
        public IList<double> SomeNumbers { get; private set; }

        [FirestoreProperty("first_sighting_location")]
        public SightingLocation FirstSightingLocation { get; private set; }

        [FirestoreProperty("items")]
        public IList<SimpleItem> Items { get; private set; }

        [FirestoreProperty("creation_date")]
        public DateTime CreationDate { get; private set; }

        [FirestoreServerTimestamp("server_timestamp")]
        public DateTimeOffset ServerTimestamp { get; private set; }

        [FirestoreProperty("original_reference")]
        public IDocumentReference OriginalReference { get; private set; }
    }
}
