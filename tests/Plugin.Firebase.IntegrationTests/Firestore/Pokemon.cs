using JetBrains.Annotations;
using Plugin.Firebase.Core.Extensions;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed class Pokemon : IFirestoreObject
{
    internal const string NameField = "name";
    internal const string SightingCountField = "sighting_count";
    internal const string PokeTypeField = "poke_type";
    internal const string ItemsField = "items";
    internal const string MovesField = "moves";
    internal const string FirstSightingLocationField = "first_sighting_location";
    internal const string OtherPropertiesColorsPath = "other_properties.colors";

    [Preserve]
    public Pokemon()
    {
        // needed for firestore
    }

    public Pokemon(
        string id,
        string name,
        double weightInKg = 0,
        float heightInCm = 0,
        long sightingCount = 0,
        bool isFromFirstGeneration = false,
        PokeType pokeType = default,
        IList<string>? moves = null,
        IList<double>? someNumbers = null,
        SightingLocation? firstSightingLocation = null,
        IList<SimpleItem>? items = null,
        IDictionary<string, long>? otherProperties = null,
        IDocumentReference? originalReference = null)
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
        CreationDate = DateTime.Now;
        Items = items;
        OriginalReference = originalReference;
        OtherProperties = otherProperties;
    }

    /// <summary>
    /// Get a clone from the current pokemon
    /// </summary>
    /// <param name="originalReference">Reference to the original document that shall be cloned</param>
    /// <returns>A copy of the current pokemon</returns>
    public Pokemon Clone(IDocumentReference originalReference)
    {
        return new Pokemon(
            id: $"{Id}_copied",
            name: Name,
            weightInKg: WeightInKg,
            heightInCm: HeightInCm,
            isFromFirstGeneration: IsFromFirstGeneration,
            pokeType: PokeType,
            moves: Moves?.ToList(),
            someNumbers: SomeNumbers?.ToList(),
            firstSightingLocation: FirstSightingLocation,
            items: Items?.ToList(),
            originalReference: originalReference);
    }

    public override bool Equals(object? obj)
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
        var hash = new HashCode();
        // ReSharper disable NonReadonlyMemberInGetHashCode
        hash.Add(Id);
        hash.Add(Name);
        hash.Add(WeightInKg);
        hash.Add(HeightInCm);
        hash.Add(SightingCount);
        hash.Add(IsFromFirstGeneration);
        hash.Add(PokeType);
        hash.Add(FirstSightingLocation);
        AddSequenceToHash(ref hash, Moves);
        AddSequenceToHash(ref hash, SomeNumbers);
        AddSequenceToHash(ref hash, Items);
        return hash.ToHashCode();
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }

    public override string ToString()
    {
        return $"[{nameof(Pokemon)}: {nameof(Id)}={Id}, {nameof(Name)}={Name}]";
    }

    [FirestoreDocumentId]
    public string Id { get; [UsedImplicitly] private set; } = null!;

    [FirestoreProperty(NameField)]
    public string Name { get; [UsedImplicitly] private set; } = null!;

    [FirestoreProperty("weight_in_kg")]
    public double WeightInKg { get; [UsedImplicitly] private set; }

    [FirestoreProperty("height_in_cm")]
    public float HeightInCm { get; [UsedImplicitly] private set; }

    [FirestoreProperty(SightingCountField)]
    public long SightingCount { get; [UsedImplicitly] private set; }

    [FirestoreProperty("is_from_first_generation")]
    public bool IsFromFirstGeneration { get; [UsedImplicitly] private set; }

    [FirestoreProperty(PokeTypeField)]
    public PokeType PokeType { get; [UsedImplicitly] private set; }

    [FirestoreProperty(MovesField)]
    public IList<string>? Moves { get; [UsedImplicitly] private set; }

    [FirestoreProperty("some_numbers")]
    public IList<double>? SomeNumbers { get; [UsedImplicitly] private set; }

    [FirestoreProperty(FirstSightingLocationField)]
    public SightingLocation? FirstSightingLocation { get; [UsedImplicitly] private set; }

    [FirestoreProperty(ItemsField)]
    public IList<SimpleItem>? Items { get; [UsedImplicitly] private set; }

    [FirestoreProperty("creation_date")]
    public DateTime CreationDate { get; private set; }

    [FirestoreServerTimestamp("server_timestamp")]
    public DateTimeOffset ServerTimestamp { get; [UsedImplicitly] private set; }

    [FirestoreProperty("original_reference")]
    public IDocumentReference? OriginalReference { [UsedImplicitly] get; private set; }

    [FirestoreProperty("other_properties")]
    public IDictionary<string, long>? OtherProperties { get; [UsedImplicitly] set; }

    private static void AddSequenceToHash<T>(ref HashCode hash, IEnumerable<T>? values)
    {
        if(values == null) {
            hash.Add(0);
            return;
        }

        foreach(var value in values) {
            hash.Add(value);
        }
    }
}