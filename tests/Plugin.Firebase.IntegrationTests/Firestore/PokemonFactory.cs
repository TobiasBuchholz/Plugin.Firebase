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
                firstSightingLocation:new SightingLocation(52.5042112, 13.5290173),
                evolutions:new List<Pokemon> { new Pokemon("1", "Bulbasur"), new Pokemon("2", "Ivysaur"), new Pokemon("3", "Venusaur") });
        }
        
        public static Pokemon CreateIvysaur()
        {
            return new Pokemon(
                id:"2",
                name:"Ivysaur",
                weightInKg:13.0,
                heightInCm:100,
                isFromFirstGeneration:true,
                pokeType:PokeType.Plant,
                moves:new List<string> { "Razor-Wind", "Swords-Dance", "Cut" },
                firstSightingLocation:new SightingLocation(52.5042112, 13.5290173),
                evolutions:new List<Pokemon> { new Pokemon("1", "Bulbasur"), new Pokemon("2", "Ivysaur"), new Pokemon("3", "Venusaur") });
        }
        
        public static Pokemon CreateVenusaur()
        {
            return new Pokemon(
                id:"3",
                name:"Venusaur",
                weightInKg:100.0,
                heightInCm:200,
                isFromFirstGeneration:true,
                pokeType:PokeType.Plant,
                moves:new List<string> { "Razor-Wind", "Swords-Dance", "Cut" },
                firstSightingLocation:new SightingLocation(52.5042112, 13.5290173),
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
                firstSightingLocation:new SightingLocation(52.5042112, 13.5290173),
                evolutions:new List<Pokemon> { new Pokemon("4", "Charmander"), new Pokemon("5", "Charmeleon"), new Pokemon("6", "Charizard") });
        }
        
        public static Pokemon CreateCharmeleon()
        {
            return new Pokemon(
                id:"5",
                name:"Charmeleon",
                weightInKg:19.0,
                heightInCm:110,
                isFromFirstGeneration:true,
                pokeType:PokeType.Fire,
                moves:new List<string> { "Scratch", "Body-Slam", "Fire-Punch" },
                firstSightingLocation:new SightingLocation(52.5042112, 13.5290173),
                evolutions:new List<Pokemon> { new Pokemon("4", "Charmander"), new Pokemon("5", "Charmeleon"), new Pokemon("6", "Charizard") });
        }
        
        public static Pokemon CreateCharizard()
        {
            return new Pokemon(
                id:"6",
                name:"Charizard",
                weightInKg:90.5,
                heightInCm:170,
                isFromFirstGeneration:true,
                pokeType:PokeType.Fire,
                moves:new List<string> { "Scratch", "Body-Slam", "Fire-Punch" },
                firstSightingLocation:new SightingLocation(52.5042112, 13.5290173),
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
                firstSightingLocation:new SightingLocation(52.5042112, 13.5290173),
                evolutions:new List<Pokemon> { new Pokemon("7", "Sqirtle"), new Pokemon("8", "Wartortle"), new Pokemon("9", "Blastoise") });
        }
        
        public static Pokemon CreateWartortle()
        {
            return new Pokemon(
                id:"8",
                name:"Wartortle",
                weightInKg:22.5,
                heightInCm:100,
                isFromFirstGeneration:true,
                pokeType:PokeType.Water,
                moves:new List<string> { "Tackel", "Body-Slam", "Water-Gun" },
                firstSightingLocation:new SightingLocation(52.5042112, 13.5290173),
                evolutions:new List<Pokemon> { new Pokemon("7", "Sqirtle"), new Pokemon("8", "Wartortle"), new Pokemon("9", "Blastoise") });
        }
        
        public static Pokemon CreateBlastoise()
        {
            return new Pokemon(
                id:"9",
                name:"Blastoise",
                weightInKg:85.5,
                heightInCm:160,
                isFromFirstGeneration:true,
                pokeType:PokeType.Water,
                moves:new List<string> { "Tackel", "Body-Slam", "Water-Gun" },
                firstSightingLocation:new SightingLocation(52.5042112, 13.5290173),
                evolutions:new List<Pokemon> { new Pokemon("7", "Sqirtle"), new Pokemon("8", "Wartortle"), new Pokemon("9", "Blastoise") });
        }
    }
}