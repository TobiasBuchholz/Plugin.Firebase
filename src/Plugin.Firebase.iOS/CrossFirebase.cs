using Firebase.Core;

namespace Plugin.Firebase.iOS
{
    public static class CrossFirebase
    {
        public static void Initialize()
        {
            App.Configure();
            ProjectId = App.DefaultInstance.Options.ProjectId;
        } 
        
        public static string ProjectId { get; private set; }
    }
}