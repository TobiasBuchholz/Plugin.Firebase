namespace Plugin.Firebase.AppCheck;

/// <summary>
/// Defines the native AppCheck provider to install.
/// </summary>
public enum AppCheckProviderType
{
    /// <summary>Do not install an AppCheck provider.</summary>
    Disabled,
    /// <summary>Debug provider.</summary>
    Debug,
    /// <summary>Apple DeviceCheck provider (iOS only).</summary>
    DeviceCheck,
    /// <summary>Apple App Attest provider (iOS only).</summary>
    AppAttest,
    /// <summary>Play Integrity provider (Android only).</summary>
    PlayIntegrity
}

/// <summary>
/// Options for configuring Firebase AppCheck.
/// </summary>
public sealed class AppCheckOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppCheckOptions"/> class.
    /// </summary>
    /// <param name="provider">The AppCheck provider to use.</param>
    public AppCheckOptions(AppCheckProviderType provider)
    {
        Provider = provider;
    }

    /// <summary>
    /// Gets the AppCheck provider.
    /// </summary>
    public AppCheckProviderType Provider { get; }

    /// <summary>Gets the disabled AppCheck options.</summary>
    public static AppCheckOptions Disabled { get; } = new(AppCheckProviderType.Disabled);
    /// <summary>Gets the debug AppCheck options.</summary>
    public static AppCheckOptions Debug { get; } = new(AppCheckProviderType.Debug);
    /// <summary>Gets the DeviceCheck AppCheck options.</summary>
    public static AppCheckOptions DeviceCheck { get; } = new(AppCheckProviderType.DeviceCheck);
    /// <summary>Gets the AppAttest AppCheck options.</summary>
    public static AppCheckOptions AppAttest { get; } = new(AppCheckProviderType.AppAttest);
    /// <summary>Gets the PlayIntegrity AppCheck options.</summary>
    public static AppCheckOptions PlayIntegrity { get; } = new(AppCheckProviderType.PlayIntegrity);
}