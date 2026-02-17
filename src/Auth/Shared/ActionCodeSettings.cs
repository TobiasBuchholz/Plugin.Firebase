namespace Plugin.Firebase.Auth;

/// <summary>
/// Settings for configuring action code operations such as password reset, email verification, and email link sign-in.
/// </summary>
public sealed class ActionCodeSettings
{
    /// <summary>
    /// Sets the Android package name for the action code.
    /// </summary>
    /// <param name="packageName">The Android package name of the app.</param>
    /// <param name="installIfNotAvailable">Whether to install the Android app if not available.</param>
    /// <param name="minimumVersion">The minimum Android app version required.</param>
    public void SetAndroidPackageName(
        string packageName,
        bool installIfNotAvailable,
        string minimumVersion
    )
    {
        AndroidPackageName = packageName;
        AndroidInstallIfNotAvailable = installIfNotAvailable;
        AndroidMinimumVersion = minimumVersion;
    }

    /// <summary>
    /// Gets or sets the URL to continue to after the action code is handled.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the iOS bundle ID associated with the app.
    /// </summary>
    public string? IOSBundleId { get; set; }

    /// <summary>
    /// Gets the Android package name of the app.
    /// </summary>
    public string? AndroidPackageName { get; private set; }

    /// <summary>
    /// Gets the minimum app version required.
    /// </summary>
    public string? AndroidMinimumVersion { get; private set; }

    /// <summary>
    /// Gets whether to install the Android app if not available.
    /// </summary>
    public bool AndroidInstallIfNotAvailable { get; private set; }

    /// <summary>
    /// Gets or sets whether the action code link will open in a mobile app or web browser.
    /// </summary>
    public bool HandleCodeInApp { get; set; }
}