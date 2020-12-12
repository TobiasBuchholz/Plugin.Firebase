using System;
using System.Collections.Generic;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;

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
            bool isFromFirstGeneration = false,
            PokeType pokeType = default(PokeType),
            IList<string> moves = null,
            SightLocation firstSightLocation = null,
            IList<Pokemon> evolutions = null,
            DateTimeOffset creationDate = default(DateTimeOffset))
        {
            Id = id;
            Name = name;
            WeightInKg = weightInKg;
            HeightInCm = heightInCm;
            IsFromFirstGeneration = isFromFirstGeneration;
            PokeType = pokeType;
            Moves = moves;
            FirstSightLocation = firstSightLocation;
            Evolutions = evolutions;
            CreationDate = creationDate;
        }
        
        public override bool Equals(object obj)
        {
            if(obj is Pokemon other) {
                return (Id, Name, WeightInKg, HeightInCm, IsFromFirstGeneration, PokeType, Moves, FirstSightLocation, Evolutions, CreationDate)
                    .Equals((other.Id, other.Name, other.WeightInKg, other.HeightInCm, other.IsFromFirstGeneration, other.PokeType, other.Moves, other.FirstSightLocation, other.Evolutions, other.CreationDate));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (Id, Name, WeightInKg, HeightInCm, IsFromFirstGeneration, PokeType, Moves, FirstSightLocation, Evolutions, CreationDate).GetHashCode();
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