namespace Plugin.Firebase.Installations;

/// <summary>
/// Cross-platform Firebase Installations service.
/// </summary>
public sealed class CrossFirebaseInstallations
{
    private static Lazy<IFirebaseInstallations> _implementation = new(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

    private static IFirebaseInstallations CreateInstance()
    {
#if IOS || ANDROID
        return new FirebaseInstallationsImplementation();
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
    public static IFirebaseInstallations Current {
        get {
            var ret = _implementation.Value;
            if(ret == null) {
                throw NotImplementedInReferenceAssembly();
            }
            return ret;
        }
    }

    /// <summary>
    /// Gets the Firebase installation ID for the current app instance.
    /// </summary>
    /// <returns>The Firebase installation ID.</returns>
    public static Task<string> GetIdAsync()
    {
        return Current.GetIdAsync();
    }

    /// <summary>
    /// Gets a Firebase Installations auth token for the current app instance.
    /// </summary>
    /// <param name="forceRefresh">If true, bypasses cached tokens when possible.</param>
    /// <returns>The Firebase Installations auth token string.</returns>
    public static Task<string> GetTokenAsync(bool forceRefresh = false)
    {
        return Current.GetTokenAsync(forceRefresh);
    }

    /// <summary>
    /// Deletes the current Firebase installation data from the client and Firebase backend.
    /// </summary>
    public static Task DeleteAsync()
    {
        return Current.DeleteAsync();
    }

    private static Exception NotImplementedInReferenceAssembly() =>
        new NotImplementedException("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");

    /// <summary>
    /// Dispose of everything
    /// </summary>
    public static void Dispose()
    {
        if(_implementation is { IsValueCreated: true }) {
            _implementation.Value.Dispose();
            _implementation = new Lazy<IFirebaseInstallations>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);
        }
    }
}