using System;
using System.Reflection;
using Foundation;
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Firestore;
using UIKit;
using Plugin.Firebase.IntegrationTests;
using Xunit.Runner;
using Xunit.Sdk;
using FirebaseApp = Firebase.Core.App;

namespace Plugin.Firebase.TestHarness.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("IntegrationTestAppDelegate")]
    public partial class IntegrationTestAppDelegate : RunnerAppDelegate
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
            FirebaseApp.Configure();
            FirebaseAnalyticsImplementation.Initialize();
	        
            // We need this to ensure the execution assembly is part of the app bundle
            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
            
            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());
            // otherwise you need to ensure that the test assemblies will 
            // become part of the app bundle
            AddTestAssembly(typeof(MustPassFixture).Assembly);

            // Uncomment this line in if you are running the Firebase Emulator Suite
            // CrossFirebaseFirestore.Current.UseEmulator("localhost", 8080);
            
            return base.FinishedLaunching(app, options);
		}
    }
}
