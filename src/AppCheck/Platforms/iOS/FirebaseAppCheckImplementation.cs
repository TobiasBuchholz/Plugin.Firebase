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
    // The native provider factory is process-wide, so the provider installed before the default app was configured is
    // shared by every implementation instance, including one created after CrossFirebaseAppCheck.Dispose().
    private static readonly object SyncRoot = new();
    private static AppCheckProviderType? InstalledProvider;
    private AppCheckOptions _options = AppCheckOptions.Disabled;
    private IDisposable? _beforeConfigureRegistration;

    /// <summary>
    /// Configures the Firebase AppCheck service with the specified options.
    /// </summary>
    /// <remarks>
    /// The native SDK reads the provider factory when Firebase configures an app, so call this before
    /// <c>CrossFirebase.Initialize()</c>. Once the default app is configured, configuring the provider already in
    /// effect does nothing and any other provider throws.
    /// </remarks>
    /// <param name="options">The AppCheck configuration options.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="options"/> selects a provider iOS does not support.</exception>
    /// <exception cref="InvalidOperationException">
    /// The default Firebase app is already configured and <paramref name="options"/> selects a provider other than
    /// the one in effect.
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

        lock(SyncRoot) {
            // The App Check component is created eagerly when an app is configured and reads the provider factory
            // then, so a factory set afterwards never reaches the default app. Asking the native SDK also covers apps
            // configured without CrossFirebase.Initialize(), and accepts a new provider again after an
            // initialization failed or the default app was deleted.
            if(IsDefaultAppConfigured()) {
                // The plugin only tracks the factory it set itself, and treats Disabled as in effect when it set none.
                var providerInEffect = InstalledProvider ?? AppCheckProviderType.Disabled;
                if(options.Provider == providerInEffect) {
                    return;
                }

                throw new InvalidOperationException(
                    $"App Check cannot change from the '{providerInEffect}' provider to '{options.Provider}' after the "
                        + "default Firebase app is configured, because the native Firebase iOS SDK reads the provider "
                        + "factory only when an app is configured. Call CrossFirebaseAppCheck.Configure() before "
                        + "CrossFirebase.Initialize()."
                );
            }

            _options = options;
        }

        // Always register the BeforeConfigure hook, even for Disabled.
        // When the native FirebaseAppCheck framework is linked into the app binary,
        // its component auto-registers with Firebase using eager instantiation.
        // Without an explicit call to SetAppCheckProviderFactory(null), the SDK may
        // use a default DeviceCheckProviderFactory which produces placeholder tokens
        // on failure. Auth/Functions then attach these invalid tokens to every request,
        // causing server-side rejection ("The supplied auth credential is malformed
        // or has expired"). Calling SetAppCheckProviderFactory(null) before
        // FirebaseApp.configure() ensures initWithApp: returns nil and no App Check
        // tokens are attached to requests.
        _beforeConfigureRegistration ??= FirebaseInitializationHooks.RegisterBeforeConfigure(InstallProviderFactory);
    }

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

    private void InstallProviderFactory()
    {
        AppCheckOptions options;
        lock(SyncRoot) {
            options = _options;
        }

        Console.WriteLine($"Plugin.Firebase AppCheck: installing provider '{options.Provider}' (iOS).");

        switch(options.Provider) {
            case AppCheckProviderType.Disabled:
                // Explicitly clear the native factory so that FIRAppCheck.initWithApp:
                // returns nil. This prevents Auth/Functions from attaching invalid
                // placeholder App Check tokens to requests.
                global::Firebase.AppCheck.AppCheck.SetAppCheckProviderFactory(null);
                break;

            case AppCheckProviderType.Debug:
                var debugFactory = new AppCheckDebugProviderFactory();
                global::Firebase.AppCheck.AppCheck.SetAppCheckProviderFactory(debugFactory);
                break;

            case AppCheckProviderType.DeviceCheck:
                var deviceCheckFactory = new DeviceCheckProviderFactory();
                global::Firebase.AppCheck.AppCheck.SetAppCheckProviderFactory(deviceCheckFactory);
                break;

            case AppCheckProviderType.AppAttest:
                var appAttestAdapter = new AppAttestProviderFactoryAdapter();
                global::Firebase.AppCheck.AppCheck.SetAppCheckProviderFactory(appAttestAdapter);
                break;

            case AppCheckProviderType.PlayIntegrity:
                throw new NotSupportedException("AppCheck Play Integrity provider is not supported on iOS.");

            default:
                break;
        }

        lock(SyncRoot) {
            InstalledProvider = options.Provider;
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
    public void Dispose()
    {
        _beforeConfigureRegistration?.Dispose();
        _beforeConfigureRegistration = null;
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