using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Firebase;
using Playground.Common.Services.Composition;
using Playground.Common.Services.Logging;
using Playground.Common.Services.Scheduler;
using Playground.Droid.Services.Composition;
using Plugin.CurrentActivity;
using Xamarin.Forms;

namespace Playground.Droid
{
    [Activity(Label = "Playground", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Forms.SetFlags("Markup_Experimental");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            LogOutputService.Initialize();
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            FirebaseApp.InitializeApp(this);

            var compositionRoot = new CompositionRoot();
            ViewModelResolver.Initialize(compositionRoot);
            Schedulers.Initialize(compositionRoot.ResolveSchedulerService());
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}