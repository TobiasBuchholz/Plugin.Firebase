namespace Playground.Common.Services.Preferences
{
    public sealed class PreferencesService : IPreferencesService
    {
        public bool ContainsKey(string key) => Microsoft.Maui.Storage.Preferences.ContainsKey(key);
        public void Remove(string key) => Microsoft.Maui.Storage.Preferences.Remove(key);
        public void Clear() => Microsoft.Maui.Storage.Preferences.Clear();

        public string GetString(string key, string defaultValue = null) => Microsoft.Maui.Storage.Preferences.Get(key, defaultValue);
        public bool GetBool(string key, bool defaultValue = false) => Microsoft.Maui.Storage.Preferences.Get(key, defaultValue);
        public long GetLong(string key, long defaultValue = 0) => Microsoft.Maui.Storage.Preferences.Get(key, defaultValue);

        public void Set(string key, string value) => Microsoft.Maui.Storage.Preferences.Set(key, value);
        public void Set(string key, bool value) => Microsoft.Maui.Storage.Preferences.Set(key, value);
        public void Set(string key, long value) => Microsoft.Maui.Storage.Preferences.Set(key, value);
    }
}