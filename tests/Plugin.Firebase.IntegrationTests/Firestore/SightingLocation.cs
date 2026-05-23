using JetBrains.Annotations;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed class SightingLocation : IFirestoreObject
{
    [Preserve]
    [UsedImplicitly]
    public SightingLocation()
    {
        // needed for firestore
    }

    public SightingLocation(
        double latitude = 0,
        double longitude = 0)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public override bool Equals(object? obj)
    {
        if(obj is SightingLocation other) {
            return (Latitude, Longitude).Equals((other.Latitude, other.Longitude));
        }
        return false;
    }

    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return (Latitude, Longitude).GetHashCode();
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }

    public override string ToString()
    {
        return $"[{nameof(SightingLocation)}: {nameof(Latitude)}={Latitude}, {nameof(Longitude)}={Longitude}]";
    }

    [FirestoreProperty("latitude")]
    public double Latitude { get; [UsedImplicitly] private set; }

    [FirestoreProperty("longitude")]
    public double Longitude { get; [UsedImplicitly] private set; }
}