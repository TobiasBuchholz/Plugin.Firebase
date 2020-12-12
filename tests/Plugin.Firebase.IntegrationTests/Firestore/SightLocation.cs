using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed class SightLocation : IFirestoreObject
    {
        public SightLocation()
        {
            // needed for firestore
        }

        public SightLocation(
            double latitude = 0,
            double longitude = 0)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override bool Equals(object obj)
        {
            if(obj is SightLocation other) {
                return (Latitude, Longitude).Equals((other.Latitude, other.Longitude));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (Latitude, Longitude).GetHashCode();
        }

        public override string ToString()
        {
            return $"[{nameof(SightLocation)}: {nameof(Latitude)}={Latitude}, {nameof(Longitude)}]";
        }
        
        [FirestoreProperty("latitude")]
        public double Latitude { get; private set; }
        
        [FirestoreProperty("longitude")]
        public double Longitude { get; private set; }
    }
}