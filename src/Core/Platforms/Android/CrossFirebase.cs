using Firebase;

namespace Plugin.Firebase.Core.Platforms.Android;

public static class CrossFirebase
{
    public static Func<Activity> ActivityLocator { get; private set; }

    /// <param name="activity">The current activity. This will be passed into the platform FirebaseApp.Initialize method.</param>
    /// <param name="activityLocator">A delegate that should always return the 'current' activity. This will be invoked whenever the plugin needs to pass an activity into native Firebases SDK methods.</param>
    /// <param name="firebaseOptions"></param>
    /// <param name="name"></param>
    public static void Initialize(
        Activity activity,
        Func<Activity> activityLocator,
        FirebaseOptions firebaseOptions = null,
        string name = null
    )
    {
        RegisterActivityLocator(activityLocator);

        if(firebaseOptions == null) {
            FirebaseApp.InitializeApp(activity);
        } else if(name == null) {
            FirebaseApp.InitializeApp(activity, firebaseOptions);
        } else {
            FirebaseApp.InitializeApp(activity, firebaseOptions, name);
        }

        // Only invoke after-initialize hooks if the default Firebase app was successfully created.
        if(TryGetDefaultApp(out _)) {
            FirebaseInitializationHooks.InvokeAfterInitialize();
        } else {
            Console.WriteLine(
                "[Plugin.Firebase.Core] Warning: FirebaseApp.Initialize() did not create a default Firebase app instance. "
                    + "Likely cause: google-services.json is missing, corrupt, or has a package_name that doesn't match ApplicationId. "
                    + "Provide explicit FirebaseOptions to CrossFirebase.Initialize() to fix. "
                    + "AppCheck and other lifecycle-dependent services will not be initialized."
            );
        }
    }

    /// <summary>
    /// Safely checks whether a default FirebaseApp exists.
    /// FirebaseApp.Instance (getInstance()) throws IllegalStateException when no default app is configured.
    /// </summary>
    public static bool TryGetDefaultApp(out FirebaseApp app)
    {
        try {
            app = FirebaseApp.Instance;
            return app != null;
        } catch(Java.Lang.IllegalStateException) {
            app = null;
            return false;
        }
    }

    public static void RegisterActivityLocator(Func<Activity> activityLocator)
    {
        ActivityLocator = activityLocator;
    }
}