using Firebase;

namespace Plugin.Firebase.Core.Platforms.Android;

public static class CrossFirebase
{
    public static Func<Activity> ActivityLocator
    {
        get;
        private set;
    }

    /// <param name="activity">The current activity. This will be passed into the platform FirebaseApp.Initialize method.</param>
    /// <param name="activityLocator">A delegate that should always return the 'current' activity. This will be invoked whenever the plugin needs to pass an activity into native Firebases SDK methods.</param>
    /// <param name="firebaseOptions"></param>
    /// <param name="name"></param>
    public static void Initialize(
        Activity activity,
        Func<Activity> activityLocator,
        FirebaseOptions firebaseOptions = null,
        string name = null)
    {
        ActivityLocator = activityLocator;

        if(firebaseOptions == null) {
            FirebaseApp.InitializeApp(activity);
        } else if(name == null) {
            FirebaseApp.InitializeApp(activity, firebaseOptions);
        } else {
            FirebaseApp.InitializeApp(activity, firebaseOptions, name);
        }
    }
}