namespace Plugin.Firebase.AppCheck;

/// <summary>
/// Cross-platform Firebase AppCheck service.
/// </summary>
public sealed class CrossFirebaseAppCheck
{
    private static Lazy<IFirebaseAppCheck?> _implementation = new Lazy<IFirebaseAppCheck?>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

    private static IFirebaseAppCheck? CreateInstance()
    {
#if IOS || ANDROID
        return new FirebaseAppCheckImplementation();
#else
#pragma warning disable IDE0022 // Use expression body for methods
        return null;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
    }

    /// <summary>
    /// Gets if the plugin is supported on the current platform.
    /// </summary>
    public static bool IsSupported => _implementation.Value != null;

    /// <summary>
    /// Current plugin implementation to use
    /// </summary>
    public static IFirebaseAppCheck Current {
        get {
            var ret = _implementation.Value;
            if(ret == null) {
                throw NotImplementedInReferenceAssembly();
            }
            return ret;
        }
    }

    /// <summary>
    /// Configures Firebase AppCheck with the specified options.
    /// </summary>
    /// <remarks>
    /// Call this before <c>CrossFirebase.Initialize()</c>. On Android, a later call installs the provider factory
    /// right away. On iOS, the native SDK reads the provider factory only when Firebase configures an app, so after
    /// the default app is configured, configuring the provider the plugin installed does nothing. Any other provider
    /// throws, and so does every provider when the plugin installed none, because the native default
    /// (<c>DeviceCheck</c>) or a factory the app set natively may be in effect.
    /// </remarks>
    /// <param name="options">The AppCheck options to apply.</param>
    /// <exception cref="NotSupportedException">
    /// <paramref name="options"/> selects a provider that the current platform does not support.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// On Android, <paramref name="options"/> is <see cref="AppCheckOptions.Disabled"/> while a provider factory is
    /// installed on the default Firebase app. The native SDK cannot remove an installed provider factory.
    /// On iOS, the default Firebase app is already configured, and <paramref name="options"/> selects a provider other
    /// than the one the plugin installed, or the plugin installed none. The native SDK reads the provider factory only
    /// when <c>FirebaseApp.configure()</c> runs.
    /// </exception>
    public static void Configure(AppCheckOptions options)
    {
        Current.Configure(options);
    }

    /// <summary>
    /// Fetches an App Check token for the current Firebase app instance.
    /// </summary>
    /// <param name="forceRefresh">If true, bypasses cached tokens when possible.</param>
    /// <returns>The App Check token string.</returns>
    public static Task<string> GetTokenAsync(bool forceRefresh = false)
    {
        return Current.GetTokenAsync(forceRefresh);
    }

    private static Exception NotImplementedInReferenceAssembly() =>
        new NotImplementedException("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");

    /// <summary>
    /// Dispose of everything 
    /// </summary>
    /// <remarks>
    /// The configured provider is kept, because it belongs to the default Firebase app rather than to the current
    /// instance. On Android, an installed provider factory cannot be removed. On iOS, a provider configured before
    /// initialization is still installed by the next <c>CrossFirebase.Initialize()</c>. To drop a provider before the
    /// first <c>CrossFirebase.Initialize()</c>, configure <see cref="AppCheckOptions.Disabled"/> instead.
    /// </remarks>
    public static void Dispose()
    {
        if(_implementation != null && _implementation.IsValueCreated) {
            _implementation.Value?.Dispose();
            _implementation = new Lazy<IFirebaseAppCheck?>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);
        }
    }
}