using System;
using Foundation;
using Playground.Common.Services.Composition;
using Playground.Common.Services.Logging;
using Playground.Common.Services.Scheduler;
using Playground.iOS.Services.Composition;
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Auth;
using UIKit;
using Xamarin.Forms;

namespace Playground.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Forms.SetFlags("Markup_Experimental");
            Forms.Init();
            LogOutputService.Initialize();
            Firebase.Core.App.Configure();
            FirebaseAnalyticsImplementation.Initialize();
            FirebaseAuthImplementation.Initialize(app, options, "1234567890", "playground");

            var compositionRoot = new CompositionRoot();
            ViewModelResolver.Initialize(compositionRoot);
            Schedulers.Initialize(compositionRoot.ResolveSchedulerService());
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
