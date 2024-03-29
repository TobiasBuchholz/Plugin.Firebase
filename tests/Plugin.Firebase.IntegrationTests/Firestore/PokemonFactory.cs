using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public static class PokemonFactory
    {
        public static Pokemon CreateBulbasur()
        {
            return new Pokemon(
                id: "1",
                name: "Bulbasaur",
                weightInKg: 6.9,
                heightInCm: 70,
                isFromFirstGeneration: true,
                pokeType: PokeType.Plant,
                moves: new List<string> { "Razor-Wind", "Swords-Dance", "Cut" },
                someNumbers: new List<double> { 1, 2, 3 },
                firstSightingLocation: new SightingLocation(52.5042112, 13.5290173),
                items: CreateSimpleItems("Wonder Berry", "Air Balloon"));
        }

        private static IList<SimpleItem> CreateSimpleItems(params string[] titles)
        {
            return titles.Select(title => new SimpleItem(title)).ToList();
        }

        public static Pokemon CreateIvysaur()
        {
            return new Pokemon(
                id: "2",
                name: "Ivysaur",
                weightInKg: 13.0,
                heightInCm: 100,
                isFromFirstGeneration: true,
                pokeType: PokeType.Plant,
                moves: new List<string> { "Razor-Wind", "Swords-Dance", "Cut" },
                firstSightingLocation: new SightingLocation(52.5042112, 13.5290173),
                items: CreateSimpleItems("Wonder Berry", "Air Balloon"));
        }

        public static Pokemon CreateVenusaur()
        {
            return new Pokemon(
                id: "3",
                name: "Venusaur",
                weightInKg: 100.0,
                heightInCm: 200,
                isFromFirstGeneration: true,
                pokeType: PokeType.Plant,
                moves: new List<string> { "Razor-Wind", "Swords-Dance", "Cut" },
                firstSightingLocation: new SightingLocation(52.5042112, 13.5290173),
                items: CreateSimpleItems("Apicot Berry", "Antidote"));
        }

        public static Pokemon CreateCharmander()
        {
            return new Pokemon(
                id: "4",
                name: "Charmander",
                weightInKg: 8.5,
                heightInCm: 60,
                isFromFirstGeneration: true,
                pokeType: PokeType.Fire,
                moves: new List<string> { "Scratch", "Body-Slam", "Fire-Punch" },
                firstSightingLocation: new SightingLocation(52.5042112, 13.5290173),
                items: CreateSimpleItems("Antidote", "Air Balloon"));
        }

        public static Pokemon CreateCharmeleon()
        {
            return new Pokemon(
                id: "5",
                name: "Charmeleon",
                weightInKg: 19.0,
                heightInCm: 110,
                isFromFirstGeneration: true,
                pokeType: PokeType.Fire,
                moves: new List<string> { "Scratch", "Body-Slam", "Fire-Punch" },
                firstSightingLocation: new SightingLocation(52.5042112, 13.5290173),
                items: CreateSimpleItems("Antidote"));
        }

        public static Pokemon CreateCharizard()
        {
            return new Pokemon(
                id: "6",
                name: "Charizard",
                weightInKg: 90.5,
                heightInCm: 170,
                isFromFirstGeneration: true,
                pokeType: PokeType.Fire,
                moves: new List<string> { "Scratch", "Body-Slam", "Fire-Punch" },
                firstSightingLocation: new SightingLocation(52.5042112, 13.5290173),
                items: CreateSimpleItems("Wonder Berry"));
        }

        public static Pokemon CreateSquirtle()
        {
            return new Pokemon(
                id: "7",
                name: "Squirtle",
                weightInKg: 9,
                heightInCm: 50,
                isFromFirstGeneration: true,
                pokeType: PokeType.Water,
                moves: new List<string> { "Tackel", "Body-Slam", "Water-Gun" },
                firstSightingLocation: new SightingLocation(52.5042112, 13.5290173),
                items: CreateSimpleItems("Wonder Berry", "Antidote"));
        }

        public static Pokemon CreateWartortle()
        {
            return new Pokemon(
                id: "8",
                name: "Wartortle",
                weightInKg: 22.5,
                heightInCm: 100,
                isFromFirstGeneration: true,
                pokeType: PokeType.Water,
                moves: new List<string> { "Tackel", "Body-Slam", "Water-Gun" },
                firstSightingLocation: new SightingLocation(52.5042112, 13.5290173),
                items: CreateSimpleItems("Air Balloon", "Antidote"));
        }

        public static Pokemon CreateBlastoise()
        {
            return new Pokemon(
                id: "9",
                name: "Blastoise",
                weightInKg: 85.5,
                heightInCm: 160.5f,
                isFromFirstGeneration: true,
                pokeType: PokeType.Water,
                moves: new List<string> { "Tackel", "Body-Slam", "Water-Gun" },
                firstSightingLocation: new SightingLocation(52.5042112, 13.5290173),
                items: CreateSimpleItems("Wonder Berry", "Apicot Berry"));
        }

        /// <summary>
        /// Use this method to create mock data on your firestore project.
        /// </summary>
        public static async Task CreateBasePokemonsAtFirestoreAsync()
        {
            var firestore = CrossFirebaseFirestore.Current;
            var pokemons = CreateBasePokemons();
            foreach(var pokemon in pokemons) {
                var path = $"pokemons/{pokemon.Id}";
                var document = firestore.GetDocument(path);
                await document.SetDataAsync(pokemon);
            }
        }

        private static IEnumerable<Pokemon> CreateBasePokemons()
        {
            yield return CreateBulbasur();
            yield return CreateIvysaur();
            yield return CreateVenusaur();
            yield return CreateCharmander();
            yield return CreateCharmeleon();
            yield return CreateCharizard();
            yield return CreateSquirtle();
            yield return CreateWartortle();
            yield return CreateBlastoise();
        }
    }
}
