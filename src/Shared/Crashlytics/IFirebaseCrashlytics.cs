using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.Crashlytics
{
    /// <summary>
    /// Firebase Crashlytics is a lightweight, realtime crash reporter that helps you track, prioritize, and fix stability issues that
    /// erode your app quality.
    /// </summary>
    public interface IFirebaseCrashlytics : IDisposable
    {
        /// <summary>
        /// Enables or disables the automatic data collection configuration for Crashlytics
        /// </summary>
        /// <param name="enabled">Whether to enable or disable automatic data collection</param>
        void SetCrashlyticsCollectionEnabled(bool enabled);

        /// <summary>
        /// Sets a custom key and value that are associated with subsequent fatal, non-fatal, and ANR reports.
        /// </summary>
        /// <param name="key">A unique key</param>
        /// <param name="value">A value to be associated with the given key</param>
        void SetCustomKey(string key, bool value);

        /// <summary>
        /// Sets a custom key and value that are associated with subsequent fatal, non-fatal, and ANR reports.
        /// </summary>
        /// <param name="key">A unique key</param>
        /// <param name="value">A value to be associated with the given key</param>
        void SetCustomKey(string key, int value);

        /// <summary>
        /// Sets a custom key and value that are associated with subsequent fatal, non-fatal, and ANR reports.
        /// </summary>
        /// <param name="key">A unique key</param>
        /// <param name="value">A value to be associated with the given key</param>
        void SetCustomKey(string key, long value);

        /// <summary>
        /// Sets a custom key and value that are associated with subsequent fatal, non-fatal, and ANR reports.
        /// </summary>
        /// <param name="key">A unique key</param>
        /// <param name="value">A value to be associated with the given key</param>
        void SetCustomKey(string key, float value);

        /// <summary>
        /// Sets a custom key and value that are associated with subsequent fatal, non-fatal, and ANR reports.
        /// </summary>
        /// <param name="key">A unique key</param>
        /// <param name="value">A value to be associated with the given key</param>
        void SetCustomKey(string key, double value);

        /// <summary>
        /// Sets a custom key and value that are associated with subsequent fatal, non-fatal, and ANR reports.
        /// </summary>
        /// <param name="key">A unique key</param>
        /// <param name="value">A value to be associated with the given key</param>
        void SetCustomKey(string key, string value);

        /// <summary>
        /// Sets multiple custom keys and values that are associated with subsequent fatal, non-fatal, and ANR reports.
        /// </summary>
        /// <param name="customKeysAndValues">A dictionary of keys and the values to associate with each key</param>
        void SetCustomKeys(IDictionary<string, object> customKeysAndValues);

        /// <summary>
        /// Records a user ID (identifier) that's associated with subsequent fatal, non-fatal, and ANR reports.
        /// </summary>
        /// <param name="identifier">A unique identifier for the current user</param>
        void SetUserId(string identifier);

        /// <summary>
        /// Logs a message that's included in the next fatal, non-fatal, or ANR report.
        /// </summary>
        /// <param name="message">The message to be logged</param>
        void Log(string message);

        /// <summary>
        /// Records a non-fatal report to send to Crashlytics.
        /// </summary>
        /// <param name="exception">An exception to be recorded as a non-fatal event.</param>
        void RecordException(Exception exception);

        /// <summary>
        /// Checks whether the app crashed on its previous run.
        /// </summary>
        /// <returns>True if a crash was recorded during the previous run of the app.</returns>
        bool DidCrashOnPreviousExecution();

        /// <summary>
        /// Checks a device for any fatal crash, non-fatal error, or ANR reports that haven't yet been sent to Crashlytics.
        /// </summary>
        /// <returns>The result.</returns>
        Task<bool> CheckForUnsentReportsAsync();

        /// <summary>
        /// If automatic data collection is disabled, this method queues up all the reports on a device to send to Crashlytics.
        /// Otherwise, this method is a no-op.
        /// </summary>
        void SendUnsentReports();

        /// <summary>
        /// If automatic data collection is disabled, this method queues up all the reports on a device for deletion.
        /// Otherwise, this method is a no-op.
        /// </summary>
        void DeleteUnsentReports();
    }
}