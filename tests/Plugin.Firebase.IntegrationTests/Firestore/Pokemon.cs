using System;
using System.Collections.Generic;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Shared.Common.Extensions;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed class Pokemon : IFirestoreObject
    {
        public Pokemon()
        {
            // needed for firestore
        }

        public Pokemon(
            string id = null,
            string name = null,
            double weightInKg = 0,
            long heightInCm = 0,
            long sightCount = 0,
            bool isFromFirstGeneration = false,
            PokeType pokeType = default(PokeType),
            IList<string> moves = null,
            SightLocation firstSightLocation = null,
            IList<Pokemon> evolutions = null)
        {
            Id = id;
            Name = name;
            WeightInKg = weightInKg;
            HeightInCm = heightInCm;
            SightCount = sightCount;
            IsFromFirstGeneration = isFromFirstGeneration;
            PokeType = pokeType;
            Moves = moves;
            FirstSightLocation = firstSightLocation;
            Evolutions = evolutions;
            CreationDate = DateTimeOffset.Now;
        }
        
        public override bool Equals(object obj)
        {
            if(obj is Pokemon other) {
                return (Id, Name, WeightInKg, HeightInCm, SightCount, IsFromFirstGeneration, PokeType, FirstSightLocation)
                    .Equals((other.Id, other.Name, other.WeightInKg, other.HeightInCm, other.SightCount, other.IsFromFirstGeneration, other.PokeType, other.FirstSightLocation)) &&
                    Moves.SequenceEqualSafe(other.Moves) &&
                    Evolutions.SequenceEqualSafe(other.Evolutions);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (Id, Name, WeightInKg, HeightInCm, SightCount, IsFromFirstGeneration, PokeType, Moves, FirstSightLocation, Evolutions, CreationDate).GetHashCode();
        }

        public override string ToString()
        {
            return $"[{nameof(Pokemon)}: {nameof(Id)}={Id}, {nameof(Name)}={Name}]";
        }

        [FirestoreProperty("id")]
        public string Id { get; private set; }

        [FirestoreProperty("name")]
        public string Name { get; private set; }
        
        [FirestoreProperty("weight_in_kg")]
        public double WeightInKg { get; private set; }
        
        [FirestoreProperty("height_in_cm")]
        public long HeightInCm { get; private set; }
        
        [FirestoreProperty("sight_count")]
        public long SightCount { get; private set; }
        
        [FirestoreProperty("is_from_first_generation")]
        public bool IsFromFirstGeneration { get; private set; }
        
        [FirestoreProperty("poke_type")]
        public PokeType PokeType { get; private set; }
        
        [FirestoreProperty("moves")]
        public IList<string> Moves { get; private set; }
        
        [FirestoreProperty("first_sight_location")]
        public SightLocation FirstSightLocation { get; private set; }
        
        [FirestoreProperty("evolutions")]
        public IList<Pokemon> Evolutions { get; private set; }
        
        [FirestoreProperty("creation_date")]
        public DateTimeOffset CreationDate { get; private set; }
    }
}