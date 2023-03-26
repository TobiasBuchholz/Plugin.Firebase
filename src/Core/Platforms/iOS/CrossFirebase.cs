using Firebase.Core;

namespace Plugin.Firebase.Core.Platforms.iOS;

public static class CrossFirebase
{
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