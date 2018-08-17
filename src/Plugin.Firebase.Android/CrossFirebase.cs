using Android.Content;
using Firebase;

namespace Plugin.Firebase.Android
{
    public static class CrossFirebase
    {
        public static void Initialize(Context context)
        {
            // FirebaseApp.Initialize(context) does not initialize the projectId, which leads to a crash in FirebaseFirestore.Instance 
            // this workaround will be needed until it's fixed via https://github.com/xamarin/GooglePlayServicesComponents/commit/723ebdc00867a4c70c51ad2d0dcbd36474ce8ff1
            var baseOptions = FirebaseOptions.FromResource(context);
            var options = new FirebaseOptions
                .Builder(baseOptions)
                .SetProjectId(baseOptions.StorageBucket.Split('.')[0])
                .Build();

            Current = FirebaseApp.InitializeApp(context, options, options.ProjectId);
        } 
        
        public static FirebaseApp Current { get; private set; }
    }
}