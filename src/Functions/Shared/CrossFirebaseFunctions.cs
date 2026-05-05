#nullable enable

namespace Plugin.Firebase.Functions;

/// <summary>
/// Cross-platform entry point for Firebase Functions.
/// </summary>
public sealed class CrossFirebaseFunctions
{
    private static readonly object SyncRoot = new();
    private static string? _region;
    private static Lazy<IFirebaseFunctions?> _implementation = CreateLazyImplementation();

    private static Lazy<IFirebaseFunctions?> CreateLazyImplementation() =>
        new Lazy<IFirebaseFunctions?>(CreateInstance, LazyThreadSafetyMode.PublicationOnly);

    private static IFirebaseFunctions? CreateInstance()
    {
#if IOS || ANDROID
        return _region == null
            ? new FirebaseFunctionsImplementation()
            : new FirebaseFunctionsImplementation(_region);
#else
#pragma warning disable IDE0022 // Use expression body for methods
        return null;
#pragma warning restore IDE0022 // Use expression body for methods
#endif
    }

    /// <summary>
    /// Initialize Functions with a specific region.
    /// Call this before using <see cref="Current"/>. If the region is changed after <see cref="Current"/>
    /// was already created, reacquire <see cref="Current"/> before creating new callable references.
    /// </summary>
    /// <param name="region">e.g. 'us-central1'. Pass <c>null</c> to use the default region.</param>
    public static void Initialize(string? region)
    {
        lock(SyncRoot) {
            if(_region == region) {
                return;
            }

            var emulatorSettings = GetCurrentEmulatorSettings();
            var shouldRecreateImplementation = _implementation.IsValueCreated;

            _region = region;

            if(shouldRecreateImplementation) {
                RecreateImplementation(emulatorSettings);
            }
        }
    }

    /// <summary>
    /// Gets if the plugin is supported on the current platform.
    /// </summary>
    public static bool IsSupported {
        get {
            lock(SyncRoot) {
                return _implementation.Value != null;
            }
        }
    }

    /// <summary>
    /// Current plugin implementation to use
    /// </summary>
    public static IFirebaseFunctions Current {
        get {
            lock(SyncRoot) {
                var ret = _implementation.Value;
                if(ret == null) {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }
    }

    private static Exception NotImplementedInReferenceAssembly() =>
        new NotImplementedException(
            "This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation."
        );

    /// <summary>
    /// Dispose of everything
    /// </summary>
    public static void Dispose()
    {
        lock(SyncRoot) {
            if(_implementation != null && _implementation.IsValueCreated) {
                _implementation.Value?.Dispose();
                _implementation = CreateLazyImplementation();
            }
        }
    }

    private static FirebaseFunctionsEmulatorSettings? GetCurrentEmulatorSettings()
    {
        if(!_implementation.IsValueCreated) {
            return null;
        }

        return _implementation.Value is IFirebaseFunctionsEmulatorSettingsProvider emulatorSettingsProvider
            ? emulatorSettingsProvider.EmulatorSettings
            : null;
    }

    private static void RecreateImplementation(FirebaseFunctionsEmulatorSettings? emulatorSettings)
    {
        _implementation.Value?.Dispose();
        _implementation = CreateLazyImplementation();

        if(emulatorSettings.HasValue) {
            var settings = emulatorSettings.Value;
            _implementation.Value?.UseEmulator(settings.Host, settings.Port);
        }
    }
}

internal readonly record struct FirebaseFunctionsEmulatorSettings(string Host, int Port);

internal interface IFirebaseFunctionsEmulatorSettingsProvider
{
    FirebaseFunctionsEmulatorSettings? EmulatorSettings { get; }
}