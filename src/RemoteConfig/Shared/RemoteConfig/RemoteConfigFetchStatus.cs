namespace Plugin.Firebase.RemoteConfig;

/// <summary>
/// Indicates the status of the most recent Remote Config fetch attempt.
/// </summary>
public enum RemoteConfigFetchStatus : long
{
    /// <summary>
    /// No fetch has been attempted yet.
    /// </summary>
    NoFetchYet,

    /// <summary>
    /// The last fetch was successful.
    /// </summary>
    Success,

    /// <summary>
    /// The last fetch failed.
    /// </summary>
    Failure,

    /// <summary>
    /// The last fetch was throttled due to too many requests.
    /// </summary>
    Throttled,
}