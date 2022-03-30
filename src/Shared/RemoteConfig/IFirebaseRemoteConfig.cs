using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.RemoteConfig
{
    /// <summary>
    /// Firebase Remote Config class. It can be created and used to fetch, activate and read config results and set default config results.
    /// </summary>
    public interface IFirebaseRemoteConfig : IDisposable
    {
        /// <summary>
        /// Ensures initialization is complete and clients can begin querying for Remote Config values.
        /// </summary>
        Task EnsureInitializedAsync();

        /// <summary>
        /// Config settings are custom settings.
        /// </summary>
        /// <param name="configSettings">Config settings are custom settings.</param>
        /// <returns></returns>
        Task SetRemoteConfigSettingsAsync(RemoteConfigSettings configSettings);

        /// <summary>
        /// Sets config defaults for parameter keys and values in the default namespace config.
        /// </summary>
        /// <param name="defaults">A dictionary of key value pairs representing the default values.</param>
        Task SetDefaultsAsync(IDictionary<string, object> defaults);

        /// <summary>
        /// Sets config defaults for parameter keys and values in the default namespace config.
        /// </summary>
        /// <param name="defaults">Tuples of key value pairs representing the default values.</param>
        Task SetDefaultsAsync(params (string, object)[] defaults);

        /// <summary>
        /// Fetches Remote Config data and if successful, activates fetched data.
        /// </summary>
        Task FetchAndActivateAsync();

        /// <summary>
        /// Fetches Remote Config data and sets a duration that specifies how long config data lasts.
        /// Call <c>ActivateAsync()</c> to make fetched data available to your app.
        /// </summary>
        /// <param name="expirationDuration">
        /// Override the (default or optionally set MinimumFetchInterval property in <c>RemoteConfigSettings</c>) MinimumFetchInterval for only
        /// the current request, in seconds. Setting a value of 0 seconds will force a fetch to the backend.
        /// </param>
        Task FetchAsync(double expirationDuration = 3600);

        /// <summary>
        /// Applies Fetched Config data to the Active Config, causing updates to the behavior and appearance of the app to take effect
        /// (depending on how config data is used in the app).
        /// </summary>
        Task ActivateAsync();

        /// <summary>
        /// Returns the set of parameter keys that start with the given prefix, from the default namespace in the active config.
        /// </summary>
        /// <param name="prefix">The key prefix to look for. If prefix is null or empty, returns all the keys.</param>
        IEnumerable<string> GetKeysByPrefix(string prefix);

        /// <summary>
        /// Returns the parameter value for the given key as a <c>boolean</c>.
        /// </summary>
        /// <param name="key">A Firebase Remote Config parameter key with a <c>boolean</c> parameter value.</param>
        /// <returns></returns>
        bool GetBoolean(string key);

        /// <summary>
        /// Returns the parameter value for the given key as a <c>string</c>.
        /// </summary>
        /// <param name="key">A Firebase Remote Config parameter key.</param>
        /// <returns></returns>
        string GetString(string key);

        /// <summary>
        /// Returns the parameter value for the given key as a <c>long</c>.
        /// </summary>
        /// <param name="key">A Firebase Remote Config parameter key with a <c>long</c> parameter value.</param>
        /// <returns></returns>
        long GetLong(string key);

        /// <summary>
        /// Returns the parameter value for the given key as a <c>double</c>.
        /// </summary>
        /// <param name="key">A Firebase Remote Config parameter key with a <c>double</c> parameter value.</param>
        /// <returns></returns>
        double GetDouble(string key);

        /// <summary>
        /// Wraps the current state of the FirebaseRemoteConfig singleton object.
        /// </summary>
        RemoteConfigInfo Info { get; }
    }
}