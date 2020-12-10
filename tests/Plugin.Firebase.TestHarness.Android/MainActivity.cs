using System.Reflection;
using Android.App;
using Android.OS;
using Firebase;
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Auth;
using Plugin.Firebase.IntegrationTests;
using Xamarin.Essentials;
using Xunit.Runners.UI;
using Xunit.Sdk;

namespace Plugin.Firebase.TestHarness.Android
{
    [Activity(Label = "xUnit Android Runner", MainLauncher = true, Theme = "@android:style/Theme.Material.Light")]
    public class MainActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            Platform.Init(this, savedInstanceState);
            FirebaseApp.InitializeApp(this);
            FirebaseAnalyticsImplementation.Initialize(this);
            FirebaseAuthImplementation.Initialize(this, savedInstanceState, "316652897245-lbddc4dc4v87nv3n19thi032n3dvrcvu.apps.googleusercontent.com");
            
            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());

            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
            // or in any reference assemblies			

            //AddTestAssembly(typeof(PortableTests).Assembly);
            // or in any assembly that you load (since JIT is available)
            AddTestAssembly(typeof(MustPassFixture).Assembly);

#if false
			// you can use the default or set your own custom writer (e.g. save to web site and tweet it ;-)
			Writer = new TcpTextWriter ("10.0.1.2", 16384);
			// start running the test suites as soon as the application is loaded
			AutoStart = true;
			// crash the application (to ensure it's ended) and return to springboard
			TerminateAfterExecution = true;
#endif
            // you cannot add more assemblies once calling base
            base.OnCreate(savedInstanceState);
        }
    }
}