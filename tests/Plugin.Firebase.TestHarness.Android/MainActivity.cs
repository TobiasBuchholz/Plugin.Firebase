using System.Reflection;
using Android.App;
using Android.OS;
using Firebase;
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Firestore;
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

            // Uncomment this line in if you are running the Firebase Emulator Suite
            // CrossFirebaseFirestore.Current.UseEmulator("10.0.2.2", 8080);
            
            base.OnCreate(savedInstanceState);
        }
    }
}