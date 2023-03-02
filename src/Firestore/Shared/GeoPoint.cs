namespace Plugin.Firebase.Firestore;

public class GeoPoint
{
    public GeoPoint(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }
    public double Longitude { get; }

    public override string ToString()
    {
        return $"[{nameof(Latitude)}={Latitude}, {nameof(Longitude)}={Longitude}]";
    }
}