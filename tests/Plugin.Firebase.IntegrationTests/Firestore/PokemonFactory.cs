using System;
using System.Collections.Generic;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public static class PokemonFactory
    {
        public static Pokemon CreateBulbasur()
        {
            return new Pokemon(
                id:"1",
                name:"Bulbasaur",
                weightInKg:6.9,
                heightInCm:70,
                isFromFirstGeneration:true,
                pokeType:PokeType.Plant,
                moves:new List<string> { "Razor-Wind", "Swords-Dance", "Cut" },
                firstSightLocation:new SightLocation(52.5042112, 13.5290173),
                evolutions:new List<Pokemon> { new Pokemon("1", "Bulbasur"), new Pokemon("2", "Ivysaur"), new Pokemon("3", "Venusaur") });
        }

        public static Pokemon CreateCharmander()
        {
            return new Pokemon(
                id:"4",
                name:"Charmander",
                weightInKg:8.5,
                heightInCm:60,
                isFromFirstGeneration:true,
                pokeType:PokeType.Fire,
                moves:new List<string> { "Scratch", "Body-Slam", "Fire-Punch" },
                firstSightLocation:new SightLocation(52.5042112, 13.5290173),
                evolutions:new List<Pokemon> { new Pokemon("4", "Charmander"), new Pokemon("5", "Charmeleon"), new Pokemon("6", "Charizard") });
        }

        public static Pokemon CreateSquirtle()
        {
            return new Pokemon(
                id:"7",
                name:"Squirtle",
                weightInKg:9,
                heightInCm:50,
                isFromFirstGeneration:true,
                pokeType:PokeType.Water,
                moves:new List<string> { "Tackel", "Body-Slam", "Water-Gun" },
                firstSightLocation:new SightLocation(52.5042112, 13.5290173),
                evolutions:new List<Pokemon> { new Pokemon("7", "Sqirtle"), new Pokemon("8", "Wartortle"), new Pokemon("9", "Blastoise") });
        }
    }
}