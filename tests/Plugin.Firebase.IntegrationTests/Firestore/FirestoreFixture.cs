using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Firebase.Firestore;
using Xunit;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed class FirestoreFixture
    {
        [Fact]
        public async Task adds_pokemon_to_collection()
        {
            var sut = CrossFirebaseFirestore.Current;
            var random = new Random();

            var pokemon = new Pokemon(
                id:"1",
                name:"Bulbasaur",
                weightInKg:6.9,
                heightInCm:70,
                isFromFirstGeneration:true,
                pokeType:PokeType.Plant,
                moves:new List<string> { "Razor-Wind", "Swords-Dance", "Cut" },
                firstSightLocation:new SightLocation(random.NextDouble(), random.NextDouble()),
                evolutions:new List<Pokemon> { new Pokemon("1", "Bulbasur"), new Pokemon("2", "Ivysaur"), new Pokemon("3", "Venusaur") },
                creationDate:DateTimeOffset.Now);
            
            await sut
                .GetCollection("pokemons")
                .GetDocument(pokemon.Id)
                .SetDataAsync(pokemon);
        }
    }
}