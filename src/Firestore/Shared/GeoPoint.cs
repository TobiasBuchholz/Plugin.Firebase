namespace Plugin.Firebase.Firestore;

/// <summary>
/// Represents a geographic location by its latitude and longitude.
/// </summary>
public class GeoPoint
{
    /// <summary>
    /// Creates a new <c>GeoPoint</c> with the specified latitude and longitude.
    /// </summary>
    /// <param name="latitude">The latitude value in degrees.</param>
    /// <param name="longitude">The longitude value in degrees.</param>
    public GeoPoint(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Gets the latitude value in degrees.
    /// </summary>
    public double Latitude { get; }

    /// <summary>
    /// Gets the longitude value in degrees.
    /// </summary>
    public double Longitude { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[{nameof(Latitude)}={Latitude}, {nameof(Longitude)}={Longitude}]";
    }
}