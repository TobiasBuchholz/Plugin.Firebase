namespace Plugin.Firebase.Bundled.Shared;

/// <summary>
/// Configuration settings for Plugin.Firebase bundled initialization.
/// </summary>
public sealed class CrossFirebaseSettings
{
    /// <summary>
    /// Creates a new <c>CrossFirebaseSettings</c> instance.
    /// </summary>
    /// <param name="isAnalyticsEnabled">Whether Firebase Analytics is enabled.</param>
    /// <param name="isAuthEnabled">Whether Firebase Authentication is enabled.</param>
    /// <param name="isCloudMessagingEnabled">Whether Firebase Cloud Messaging is enabled.</param>
    /// <param name="isCrashlyticsEnabled">Whether Firebase Crashlytics is enabled.</param>
    /// <param name="isDynamicLinksEnabled">Whether Firebase Dynamic Links is enabled.</param>
    /// <param name="isFirestoreEnabled">Whether Firebase Firestore is enabled.</param>
    /// <param name="isFunctionsEnabled">Whether Firebase Functions is enabled.</param>
    /// <param name="isRemoteConfigEnabled">Whether Firebase Remote Config is enabled.</param>
    /// <param name="isStorageEnabled">Whether Firebase Storage is enabled.</param>
    /// <param name="googleRequestIdToken">The Google request ID token for Google Sign-In.</param>
    /// <param name="appCheckOptions">Optional App Check options to configure App Check.</param>
    public CrossFirebaseSettings(
        bool isAnalyticsEnabled = false,
        bool isAuthEnabled = false,
        bool isCloudMessagingEnabled = false,
        bool isCrashlyticsEnabled = false,
        bool isDynamicLinksEnabled = false,
        bool isFirestoreEnabled = false,
        bool isFunctionsEnabled = false,
        bool isRemoteConfigEnabled = false,
        bool isStorageEnabled = false,
        string googleRequestIdToken = null,
        Plugin.Firebase.AppCheck.AppCheckOptions appCheckOptions = null)
    {
        IsAnalyticsEnabled = isAnalyticsEnabled;
        IsAuthEnabled = isAuthEnabled;
        IsCloudMessagingEnabled = isCloudMessagingEnabled;
        IsCrashlyticsEnabled = isCrashlyticsEnabled;
        IsDynamicLinksEnabled = isDynamicLinksEnabled;
        IsFirestoreEnabled = isFirestoreEnabled;
        IsFunctionsEnabled = isFunctionsEnabled;
        IsRemoteConfigEnabled = isRemoteConfigEnabled;
        IsStorageEnabled = isStorageEnabled;
        GoogleRequestIdToken = googleRequestIdToken;
        AppCheckOptions = appCheckOptions;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[{nameof(CrossFirebaseSettings)}: " +
               $"{nameof(IsAnalyticsEnabled)}={IsAnalyticsEnabled}," +
               $"{nameof(IsAuthEnabled)}={IsAuthEnabled}," +
               $"{nameof(IsCloudMessagingEnabled)}={IsCloudMessagingEnabled}," +
               $"{nameof(IsCrashlyticsEnabled)}={IsCrashlyticsEnabled}," +
               $"{nameof(IsDynamicLinksEnabled)}={IsDynamicLinksEnabled}," +
               $"{nameof(IsFirestoreEnabled)}={IsFirestoreEnabled}," +
               $"{nameof(IsFunctionsEnabled)}={IsFunctionsEnabled}," +
               $"{nameof(IsRemoteConfigEnabled)}={IsRemoteConfigEnabled}," +
               $"{nameof(IsStorageEnabled)}={IsStorageEnabled}," +
               $"{nameof(AppCheckOptions)}={AppCheckOptions?.Provider}]";
    }

    /// <summary>
    /// Gets whether Firebase Analytics is enabled.
    /// </summary>
    public bool IsAnalyticsEnabled { get; }

    /// <summary>
    /// Gets whether Firebase Authentication is enabled.
    /// </summary>
    public bool IsAuthEnabled { get; }

    /// <summary>
    /// Gets whether Firebase Cloud Messaging is enabled.
    /// </summary>
    public bool IsCloudMessagingEnabled { get; }

    /// <summary>
    /// Gets whether Firebase Crashlytics is enabled.
    /// </summary>
    public bool IsCrashlyticsEnabled { get; }

    /// <summary>
    /// Gets whether Firebase Dynamic Links is enabled.
    /// </summary>
    public bool IsDynamicLinksEnabled { get; }

    /// <summary>
    /// Gets whether Firebase Firestore is enabled.
    /// </summary>
    public bool IsFirestoreEnabled { get; }

    /// <summary>
    /// Gets whether Firebase Functions is enabled.
    /// </summary>
    public bool IsFunctionsEnabled { get; }

    /// <summary>
    /// Gets whether Firebase Remote Config is enabled.
    /// </summary>
    public bool IsRemoteConfigEnabled { get; }

    /// <summary>
    /// Gets whether Firebase Storage is enabled.
    /// </summary>
    public bool IsStorageEnabled { get; }

    /// <summary>
    /// Gets the Google request ID token for Google Sign-In.
    /// </summary>
    public string GoogleRequestIdToken { get; }

    /// <summary>
    /// Gets the App Check options used to configure App Check behavior.
    /// </summary>
    public Plugin.Firebase.AppCheck.AppCheckOptions AppCheckOptions { get; }
}