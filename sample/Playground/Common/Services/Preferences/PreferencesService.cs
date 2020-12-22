namespace Playground.Common.Services.Preferences
{
    public sealed class PreferencesService : IPreferencesService
    {
        public bool ContainsKey(string key) => Xamarin.Essentials.Preferences.ContainsKey(key);
        public void Remove(string key) => Xamarin.Essentials.Preferences.Remove(key);
        public void Clear() => Xamarin.Essentials.Preferences.Clear();
        
        public string GetString(string key, string defaultValue = null) => Xamarin.Essentials.Preferences.Get(key, defaultValue);
        public bool GetBool(string key, bool defaultValue = false) => Xamarin.Essentials.Preferences.Get(key, defaultValue);
        public long GetLong(string key, long defaultValue = 0) => Xamarin.Essentials.Preferences.Get(key, defaultValue);

        public void Set(string key, string value) => Xamarin.Essentials.Preferences.Set(key, value);
        public void Set(string key, bool value) => Xamarin.Essentials.Preferences.Set(key, value);
        public void Set(string key, long value) => Xamarin.Essentials.Preferences.Set(key, value);
    }
}