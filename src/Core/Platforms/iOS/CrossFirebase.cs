using Firebase.Core;

namespace Plugin.Firebase.Core.Platforms.iOS;

/// <summary>
/// iOS-specific initialization entry point for Firebase.
/// </summary>
public static class CrossFirebase
{
    /// <summary>
    /// Initializes Firebase on iOS.
    /// </summary>
    /// <param name="name">Optional name for the Firebase app instance.</param>
    /// <param name="firebaseOptions">Optional Firebase configuration options.</param>
    public static void Initialize(string name = null, Options firebaseOptions = null)
    {
        if(firebaseOptions == null) {
            App.Configure();
        } else if(name == null) {
            App.Configure(firebaseOptions);
        } else {
            App.Configure(name, firebaseOptions);
        }
    }
}