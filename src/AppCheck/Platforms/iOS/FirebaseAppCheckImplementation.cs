using System.Runtime.InteropServices;
using Firebase.AppCheck;
using Firebase.Core;
using Foundation;
using ObjCRuntime;

namespace Plugin.Firebase.AppCheck;

/// <summary>
/// iOS implementation of the Firebase AppCheck service.
/// </summary>
public sealed class FirebaseAppCheckImplementation : IFirebaseAppCheck
{
    // The native provider factory is process-wide and applies to the default Firebase app rather than to one wrapper
    // instance, so every implementation instance, including one created after CrossFirebaseAppCheck.Dispose(), shares
    // the record of the provider set for the default app.
    private static readonly AppCheckStartupProviderInstaller Installer =
        new(IsDefaultAppConfigured, SetProviderFactory);

    private static readonly IntPtr AppClassHandle = Class.GetHandle(typeof(App));
    private static readonly IntPtr IsDefaultAppConfiguredSelector = Selector.GetHandle("isDefaultAppConfigured");

    // Class methods live on the metaclass, so the selector is looked up there rather than on the class itself.
    private static readonly bool HasIsDefaultAppConfigured =
        AppClassHandle != IntPtr.Zero
            && ClassRespondsToSelector(ObjectGetClass(AppClassHandle), IsDefaultAppConfiguredSelector) != 0;

    /// <summary>
    /// Configures the Firebase AppCheck service with the specified options.
    /// </summary>
    /// <remarks>
    /// The native SDK reads the provider factory when Firebase configures an app, so call this before
    /// <c>CrossFirebase.Initialize()</c> or the native <c>App.Configure()</c>. Before the default app exists, the
    /// factory is set right away. Once the default app is configured, configuring the provider the plugin set for it
    /// does nothing. Any other provider throws, and so does every provider when the plugin set none, because the native
    /// default (<c>DeviceCheck</c>) or a factory the app set natively may be in effect.
    /// </remarks>
    /// <param name="options">The AppCheck configuration options.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="options"/> selects a provider iOS does not support.</exception>
    /// <exception cref="InvalidOperationException">
    /// The default Firebase app is already configured, and <paramref name="options"/> selects a provider other than
    /// the one the plugin set, or the plugin set none.
    /// </exception>
    public void Configure(AppCheckOptions options)
    {
        if(options == null) {
            throw new ArgumentNullException(nameof(options));
        }

        if(options.Provider is not (AppCheckProviderType.Disabled or AppCheckProviderType.Debug
            or AppCheckProviderType.DeviceCheck or AppCheckProviderType.AppAttest)) {
            throw new NotSupportedException($"AppCheck provider '{options.Provider}' is not supported on iOS.");
        }

        // A factory set while the default app is being configured on another thread may miss it. That window is the
        // native configure call itself, so configure App Check before initializing Firebase.
        Installer.Configure(options);
    }

    // Disabled clears the factory. FirebaseAppCheck installs a DeviceCheck factory as the native default when it is
    // linked, and DeviceCheck produces placeholder tokens where it fails (simulators), which Auth and Functions then
    // attach to requests and the backend rejects ("The supplied auth credential is malformed or has expired"). With no
    // factory, FIRAppCheck.initWithApp: returns nil, so no App Check tokens are attached.
    private static void SetProviderFactory(AppCheckProviderType provider)
    {
        IAppCheckProviderFactory? factory = provider switch {
            AppCheckProviderType.Disabled => null,
            AppCheckProviderType.Debug => new AppCheckDebugProviderFactory(),
            AppCheckProviderType.DeviceCheck => new DeviceCheckProviderFactory(),
            AppCheckProviderType.AppAttest => new AppAttestProviderFactoryAdapter(),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Configure() validates the provider.")
        };

        Console.WriteLine($"Plugin.Firebase AppCheck: installing provider '{provider}' (iOS).");
        global::Firebase.AppCheck.AppCheck.SetAppCheckProviderFactory(factory);
    }

    // Uses +[FIRApp isDefaultAppConfigured], the FirebaseCore-internal check that Firebase's own SDKs use. The public
    // +[FIRApp defaultApp] logs I-COR000003 when no default app exists, which is the normal state when Configure()
    // runs before CrossFirebase.Initialize(). The internal method isn't bound, so it is called through the
    // Objective-C runtime, with +defaultApp as the fallback for a FirebaseCore version that doesn't have it.
    private static bool IsDefaultAppConfigured() =>
        HasIsDefaultAppConfigured
            ? SendByteMessage(AppClassHandle, IsDefaultAppConfiguredSelector) != 0
            : App.DefaultInstance != null;

    [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "object_getClass")]
    private static extern IntPtr ObjectGetClass(IntPtr obj);

    // BOOL is one byte on every iOS ABI (bool on arm64, signed char on x86_64), so it is read as a byte.
    [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "class_respondsToSelector")]
    private static extern byte ClassRespondsToSelector(IntPtr cls, IntPtr selector);

    [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern byte SendByteMessage(IntPtr receiver, IntPtr selector);

    /// <summary>
    /// Gets a Firebase AppCheck token, optionally forcing a refresh.
    /// </summary>
    /// <param name="forceRefresh">Whether to force a refresh of the token.</param>
    /// <returns>A task that resolves to the AppCheck token string.</returns>
    public Task<string> GetTokenAsync(bool forceRefresh = false)
    {
        var taskCompletionSource = new TaskCompletionSource<string>();

        global::Firebase.AppCheck.AppCheck.SharedInstance.TokenForcingRefresh(forceRefresh, (token, error) => {
            if(error != null) {
                taskCompletionSource.TrySetException(new NSErrorException(error));
                return;
            }

            var rawToken = token?.Token;
            if(string.IsNullOrWhiteSpace(rawToken)) {
                taskCompletionSource.TrySetException(new InvalidOperationException("Firebase AppCheck returned an empty iOS token."));
                return;
            }

            taskCompletionSource.TrySetResult(rawToken);
        });

        return taskCompletionSource.Task;
    }

    /// <summary>
    /// Disposes the AppCheck implementation and cleans up resources.
    /// </summary>
    /// <remarks>
    /// The configured provider is kept, because it applies to the default Firebase app rather than to this instance.
    /// </remarks>
    public void Dispose()
    {
        // The native factory set by Configure() stays in place, so the next app configuration still reads it, as on
        // Android. Before Firebase is initialized, configuring AppCheckOptions.Disabled clears it.
    }

    private sealed class AppAttestProviderFactoryAdapter : NSObject, IAppCheckProviderFactory
    {
        public NSObject? CreateProviderWithApp(App app)
        {
            if(!OperatingSystem.IsIOSVersionAtLeast(14)) {
                return null;
            }

            return new AppAttestProvider(app);
        }
    }
}