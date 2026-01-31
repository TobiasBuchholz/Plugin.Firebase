using Firebase;

namespace Plugin.Firebase.Core.Platforms.Android;

/// <summary>
/// Android-specific initialization entry point for Firebase.
/// </summary>
public static class CrossFirebase
{
    /// <summary>
    /// Gets the activity locator delegate used to obtain the current Android activity.
    /// </summary>
    public static Func<Activity> ActivityLocator { get; private set; }

    /// <summary>
    /// Initializes Firebase on Android.
    /// </summary>
    /// <param name="activity">The current activity. This will be passed into the platform FirebaseApp.Initialize method.</param>
    /// <param name="activityLocator">A delegate that should always return the 'current' activity. This will be invoked whenever the plugin needs to pass an activity into native Firebase SDK methods.</param>
    /// <param name="firebaseOptions">Optional Firebase configuration options.</param>
    /// <param name="name">Optional name for the Firebase app instance.</param>
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
    }

    /// <summary>
    /// Registers an activity locator delegate.
    /// </summary>
    /// <param name="activityLocator">A delegate that returns the current Android activity.</param>
    public static void RegisterActivityLocator(Func<Activity> activityLocator)
    {
        ActivityLocator = activityLocator;
    }
}