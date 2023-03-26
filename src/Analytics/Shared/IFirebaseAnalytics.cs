namespace Plugin.Firebase.Analytics;

/// <summary>
/// The top level Firebase Analytics singleton that provides methods for logging events and setting user properties.
/// </summary>
public interface IFirebaseAnalytics : IDisposable
{
    /// <summary>
    /// Returns the unique ID for this instance of the application or null if ConsentType.analyticsStorage has been set to ConsentStatus.denied.
    /// </summary>
    Task<string> GetAppInstanceIdAsync();

    /// <summary>
    /// Logs an app event. The event can have up to 25 parameters. Events with the same name must have the same parameters.
    /// Up to 500 event names are supported. Using predefined events and/or parameters is recommended for optimal reporting.
    /// </summary>
    /// <param name="eventName">
    /// The name of the event. Should contain 1 to 40 alphanumeric characters or underscores. The name must start with an alphabetic
    /// character. Some event names are reserved. See FIREventNames.h for the list of reserved event names. The “firebase_”, “google_”,
    /// and “ga_” prefixes are reserved and should not be used. Note that event names are case-sensitive and that logging two events
    /// whose names differ only in case will result in two distinct events. To manually log screen view events, use the screen_view
    /// event name.
    /// </param>
    /// <param name="parameters">
    /// The dictionary of event parameters. Passing null indicates that the event has no parameters. Parameter names can be up to 40
    /// characters long and must start with an alphabetic character and contain only alphanumeric characters and underscores. Only
    /// NSString and NSNumber (signed 64-bit integer and 64-bit floating-point number) parameter types are supported. NSString
    /// parameter values can be up to 100 characters long. The “firebase_”, “google_”, and “ga_” prefixes are reserved and should not
    /// be used for parameter names.
    /// </param>
    void LogEvent(string eventName, IDictionary<string, object> parameters);

    /// <summary>
    /// Logs an app event. The event can have up to 25 parameters. Events with the same name must have the same parameters.
    /// Up to 500 event names are supported. Using predefined events and/or parameters is recommended for optimal reporting.
    /// </summary>
    /// <param name="eventName">
    /// The name of the event. Should contain 1 to 40 alphanumeric characters or underscores. The name must start with an alphabetic
    /// character. Some event names are reserved. See FIREventNames.h for the list of reserved event names. The “firebase_”, “google_”,
    /// and “ga_” prefixes are reserved and should not be used. Note that event names are case-sensitive and that logging two events
    /// whose names differ only in case will result in two distinct events. To manually log screen view events, use the screen_view
    /// event name.
    /// </param>
    /// <param name="parameters">
    /// The event parameters as tuples. Passing null indicates that the event has no parameters. Parameter names can be up to 40
    /// characters long and must start with an alphabetic character and contain only alphanumeric characters and underscores. Only
    /// NSString and NSNumber (signed 64-bit integer and 64-bit floating-point number) parameter types are supported. NSString
    /// parameter values can be up to 100 characters long. The “firebase_”, “google_”, and “ga_” prefixes are reserved and should not
    /// be used for parameter names.
    /// </param>
    void LogEvent(string eventName, params (string parameterName, object parameterValue)[] parameters);

    /// <summary>
    /// Sets the user ID property. This feature must be used in accordance with Google’s Privacy Policy
    /// </summary>
    /// <param name="id">
    /// The user ID to ascribe to the user of this app on this device, which must be non-empty and no more than 256 characters long.
    /// Setting userID to null removes the user ID.
    /// </param>
    void SetUserId(string id);

    /// <summary>
    /// Sets a user property to a given value. Up to 25 user property names are supported. Once set, user property values persist
    /// throughout the app lifecycle and across sessions.
    /// </summary>
    /// <param name="name">
    /// The name of the user property to set. Should contain 1 to 24 alphanumeric characters or underscores and must start with an
    /// alphabetic character. The “firebase_”, “google_”, and “ga_” prefixes are reserved and should not be used for user property names.
    /// </param>
    /// <param name="value">
    /// The value of the user property. Values can be up to 36 characters long. Setting the value to null removes the user property.
    /// </param>
    void SetUserProperty(string name, string value);

    /// <summary>
    /// Sets the interval of inactivity in seconds that terminates the current session. The default value is 1800 seconds (30 minutes).
    /// </summary>
    /// <param name="duration">The custom time of inactivity in seconds before the current session terminates.</param>
    void SetSessionTimoutDuration(TimeSpan duration);

    /// <summary>
    /// Clears all analytics data for this instance from the device and resets the app instance ID. FIRAnalyticsConfiguration values
    /// will be reset to the default values.
    /// </summary>
    void ResetAnalyticsData();

    /// <summary>
    /// Sets whether analytics collection is enabled for this app on this device. This setting is persisted across app sessions.
    /// By default it is enabled.
    /// </summary>
    bool IsAnalyticsCollectionEnabled { set; }
}