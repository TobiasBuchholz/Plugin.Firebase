namespace Plugin.Firebase.AppCheck;

/// <summary>
/// Defines the AppCheck mode for the Firebase AppCheck service.
/// </summary>
public enum AppCheckMode
{
    /// <summary>AppCheck is disabled.</summary>
    Disabled,
    /// <summary>AppCheck is in debug mode.</summary>
    Debug,
    /// <summary>AppCheck is in production mode.</summary>
    Production
}

/// <summary>
/// Options for configuring Firebase AppCheck.
/// </summary>
public sealed class AppCheckOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppCheckOptions"/> class.
    /// </summary>
    /// <param name="mode">The AppCheck mode to use.</param>
    public AppCheckOptions(AppCheckMode mode)
    {
        Mode = mode;
    }

    /// <summary>
    /// Gets the AppCheck mode.
    /// </summary>
    public AppCheckMode Mode { get; }

    /// <summary>Gets the disabled AppCheck options.</summary>
    public static AppCheckOptions Disabled { get; } = new(AppCheckMode.Disabled);
    /// <summary>Gets the debug AppCheck options.</summary>
    public static AppCheckOptions Debug { get; } = new(AppCheckMode.Debug);
    /// <summary>Gets the production AppCheck options.</summary>
    public static AppCheckOptions Production { get; } = new(AppCheckMode.Production);
}