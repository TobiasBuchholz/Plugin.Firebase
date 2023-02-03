using Foundation;
// using Plugin.Firebase.DynamicLinks;
using UIKit;

namespace Playground;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
    {
        // FirebaseDynamicLinksImplementation.ContinueUserActivity(application, userActivity, completionHandler);
        return base.ContinueUserActivity(application, userActivity, completionHandler);
    }

    public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
    {
        // FirebaseDynamicLinksImplementation.OpenUrl(application, url, options);
        return base.OpenUrl(application, url, options);
    }
}

