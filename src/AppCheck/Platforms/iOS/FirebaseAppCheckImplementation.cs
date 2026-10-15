using System.Runtime.InteropServices;
using Firebase.AppCheck;
using Firebase.Core;
using Foundation;
using ObjCRuntime;
using Plugin.Firebase.Core;

namespace Plugin.Firebase.AppCheck;

/// <summary>
/// iOS implementation of the Firebase AppCheck service.
/// </summary>
public sealed class FirebaseAppCheckImplementation : IFirebaseAppCheck
{
    // The native provider factory is process-wide and applies to the default Firebase app rather than to one wrapper
    // instance, so every implementation instance, including one created after CrossFirebaseAppCheck.Dispose(), shares
    // the configured provider, the hook that installs it, and the record of the provider installed for the default app.
    private static readonly object SyncRoot = new();
    private static AppCheckOptions ConfiguredOptions = AppCheckOptions.Disabled;
    private static bool BeforeConfigureHookRegistered;
    private static AppCheckProviderType? InstalledProvider;

    /// <summary>
    /// Configures the Firebase AppCheck service with the specified options.
    /// </summary>
    /// <remarks>
    /// The native SDK reads the provider factory when Firebase configures an app, so call this before
    /// <c>CrossFirebase.Initialize()</c>. Once the default app is configured, configuring the provider the plugin
    /// installed does nothing. Any other provider throws, and so does every provider when the plugin installed none,
    /// because the native default (<c>DeviceCheck</c>) or a factory the app set natively may be in effect.
    /// </remarks>
    /// <param name="options">The AppCheck configuration options.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="options"/> selects a provider iOS does not support.</exception>
    /// <exception cref="InvalidOperationException">
    /// The default Firebase app is already configured, and <paramref name="options"/> selects a provider other than
    /// the one the plugin installed, or the plugin installed none.
    /// </exception>
    public void Configure(AppCheckOptions options)
    {
        if(options == null) {
            throw new ArgumentNullException(nameof(options));
        }

        // Validates the provider before any state changes.
        _ = GetProviderFactoryCreator(options.Provider);

        lock(SyncRoot) {
            // The App Check component is created eagerly when an app is configured and reads the provider factory
            // then, so a factory set afterwards never reaches the default app. Asking the native SDK also covers apps
            // configured without CrossFirebase.Initialize(), and accepts a new provider again after an
            // initialization failed or the default app was deleted.
            //
            // A call that races CrossFirebase.Initialize() on another thread can land after the hook read the configured
            // provider but before the native configure call creates the default app. It is accepted, yet the default app
            // gets the factory the hook set, and the record names that provider, so later calls see the provider in
            // effect.
            if(IsDefaultAppConfigured()) {
                AppCheckLateConfigurationRules.EnsureProviderUnchanged(InstalledProvider, options.Provider);
                return;
            }

            ConfiguredOptions = options;

            // Every provider accepted before initialization registers the hook, including Disabled. FirebaseAppCheck
            // installs a DeviceCheck factory as the native default when it is linked, and DeviceCheck produces
            // placeholder tokens where it fails (simulators), which Auth and Functions then attach to requests and the
            // backend rejects ("The supplied auth credential is malformed or has expired"). Clearing the factory before
            // FirebaseApp.configure() makes FIRAppCheck.initWithApp: return nil, so no App Check tokens are attached.
            // Registered once, the hook stays registered and runs before every later CrossFirebase.Initialize().
            if(!BeforeConfigureHookRegistered) {
                _ = FirebaseInitializationHooks.RegisterBeforeConfigure(InstallProviderFactory);
                BeforeConfigureHookRegistered = true;
            }
        }
    }

    // The single list of providers iOS supports. Configure() validates with it without creating a native factory.
    private static Func<IAppCheckProviderFactory?> GetProviderFactoryCreator(AppCheckProviderType provider) =>
        provider switch {
            AppCheckProviderType.Disabled => static () => null,
            AppCheckProviderType.Debug => static () => new AppCheckDebugProviderFactory(),
            AppCheckProviderType.DeviceCheck => static () => new DeviceCheckProviderFactory(),
            AppCheckProviderType.AppAttest => static () => new AppAttestProviderFactoryAdapter(),
            _ => throw new NotSupportedException($"AppCheck provider '{provider}' is not supported on iOS.")
        };

    // Uses +[FIRApp isDefaultAppConfigured], the FirebaseCore-internal check that Firebase's own SDKs use. The public
    // +[FIRApp defaultApp] logs I-COR000003 when no default app exists, which is the normal state when Configure()
    // runs before CrossFirebase.Initialize(). The internal method isn't bound, so it is called through the
    // Objective-C runtime, with +defaultApp as the fallback for a FirebaseCore version that doesn't have it.
    private static bool IsDefaultAppConfigured()
    {
        IntPtr appClass = Class.GetHandle(typeof(App));
        IntPtr selector = Selector.GetHandle("isDefaultAppConfigured");

        // Class methods live on the metaclass, so look the selector up there rather than on the class itself.
        if(appClass != IntPtr.Zero && ClassRespondsToSelector(ObjectGetClass(appClass), selector) != 0) {
            return SendByteMessage(appClass, selector) != 0;
        }

        return App.DefaultInstance != null;
    }

    [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "object_getClass")]
    private static extern IntPtr ObjectGetClass(IntPtr obj);

    // BOOL is one byte on every iOS ABI (bool on arm64, signed char on x86_64), so it is read as a byte.
    [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "class_respondsToSelector")]
    private static extern byte ClassRespondsToSelector(IntPtr cls, IntPtr selector);

    [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    private static extern byte SendByteMessage(IntPtr receiver, IntPtr selector);

    private static void InstallProviderFactory()
    {
        AppCheckOptions options;
        lock(SyncRoot) {
            options = ConfiguredOptions;
        }

        // This runs right before the native configure call. Once the default app exists, it configures a secondary
        // app, which leaves the provider of the default app unchanged. Before that, the factory set here stays in place
        // until the default app is configured.
        var configuresDefaultApp = !IsDefaultAppConfigured();

        Console.WriteLine($"Plugin.Firebase AppCheck: installing provider '{options.Provider}' (iOS).");
        global::Firebase.AppCheck.AppCheck.SetAppCheckProviderFactory(GetProviderFactoryCreator(options.Provider)());

        if(configuresDefaultApp) {
            lock(SyncRoot) {
                InstalledProvider = options.Provider;
            }
        }
    }

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
        // A provider configured before initialization is still installed by the next CrossFirebase.Initialize(), as on
        // Android. Before the first CrossFirebase.Initialize(), configuring AppCheckOptions.Disabled replaces it.
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