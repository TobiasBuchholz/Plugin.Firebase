using System;
using System.Collections.Generic;
using System.Linq;
using Android.Runtime;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Shared.Common.Extensions;

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
            long heightInCm = 0,
            long sightingCount = 0,
            bool isFromFirstGeneration = false,
            PokeType pokeType = default(PokeType),
            IList<string> moves = null,
            SightingLocation firstSightingLocation = null,
            IList<Pokemon> evolutions = null,
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
            FirstSightingLocation = firstSightingLocation;
            Evolutions = evolutions;
            CreationDate = DateTimeOffset.Now;
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
                firstSightingLocation: this.FirstSightingLocation,
                evolutions: this.Evolutions.ToList(),
                originalReference: originalReference);
        }

        public override bool Equals(object obj)
        {
            if(obj is Pokemon other) {
                return (Id, Name, WeightInKg, HeightInCm, SightingCount, IsFromFirstGeneration, PokeType, FirstSightingLocation)
                    .Equals((other.Id, other.Name, other.WeightInKg, other.HeightInCm, other.SightingCount, other.IsFromFirstGeneration, other.PokeType, other.FirstSightingLocation)) &&
                    Moves.SequenceEqualSafe(other.Moves) &&
                    Evolutions.SequenceEqualSafe(other.Evolutions);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (Id, Name, WeightInKg, HeightInCm, SightingCount, IsFromFirstGeneration, PokeType, Moves, FirstSightingLocation, Evolutions, CreationDate).GetHashCode();
        }

        public override string ToString()
        {
            return $"[{nameof(Pokemon)}: {nameof(Id)}={Id}, {nameof(Name)}={Name}]";
        }

        [FirestoreDocumentId]
        [FirestoreProperty("id")] // this attribute is still needed for Pokemon items in Evolutions list
        public string Id { get; private set; }

        [FirestoreProperty("name")]
        public string Name { get; private set; }

        [FirestoreProperty("weight_in_kg")]
        public double WeightInKg { get; private set; }

        [FirestoreProperty("height_in_cm")]
        public long HeightInCm { get; private set; }

        [FirestoreProperty("sighting_count")]
        public long SightingCount { get; private set; }

        [FirestoreProperty("is_from_first_generation")]
        public bool IsFromFirstGeneration { get; private set; }

        [FirestoreProperty("poke_type")]
        public PokeType PokeType { get; private set; }

        [FirestoreProperty("moves")]
        public IList<string> Moves { get; private set; }

        [FirestoreProperty("first_sighting_location")]
        public SightingLocation FirstSightingLocation { get; private set; }

        [FirestoreProperty("evolutions")]
        public IList<Pokemon> Evolutions { get; private set; }

        [FirestoreProperty("creation_date")]
        public DateTimeOffset CreationDate { get; private set; }

        [FirestoreProperty("original_reference")]
        public IDocumentReference OriginalReference { get; private set; }
    }
}