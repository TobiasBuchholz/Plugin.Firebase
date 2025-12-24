namespace Plugin.Firebase.Firestore;

/// <summary>
/// Settings used to configure a <c>IFirebaseFirestore</c> instance.
/// </summary>
public sealed class FirestoreSettings
{
    public FirestoreSettings(
        string host = null,
        bool isSslEnabled = false)
    {
        Host = host;
        IsSslEnabled = isSslEnabled;
    }

    /// <summary>
    /// Returns the host of the Cloud Firestore backend.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Returns whether or not to use SSL for communication.
    /// </summary>
    public bool IsSslEnabled { get; }
}