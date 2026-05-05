namespace Plugin.Firebase.Analytics;

/// <summary>
/// The type of Firebase Analytics consent to set.
/// </summary>
public enum ConsentType
{
    /// <summary>
    /// Consent for ad storage.
    /// </summary>
    AdStorage,

    /// <summary>
    /// Consent for analytics storage.
    /// </summary>
    AnalyticsStorage,

    /// <summary>
    /// Consent for ad user data.
    /// </summary>
    AdUserData,

    /// <summary>
    /// Consent for ad personalization.
    /// </summary>
    AdPersonalization
}
