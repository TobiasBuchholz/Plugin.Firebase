using Xunit.Sdk;
#if ANDROID
using AndroidRuntime = Android.Runtime;
#endif

namespace Plugin.Firebase.IntegrationTests;

internal enum IntegrationTestBackend
{
    Emulator,
    Real
}

internal readonly record struct EmulatorEndpoint(string Host, int Port);

internal static class IntegrationTestEnvironment
{
    public const string ProjectId = "demo-pluginfirebase-integrationtests";
    public const string StorageBucket = "demo-pluginfirebase-integrationtests.appspot.com";
    public const string DatabaseUrl = "https://demo-pluginfirebase-integrationtests.firebaseio.com";
    public const string ApiKey = "AIzaSyD00000000000000000000000000000000";
    public const string GcmSenderId = "123456789012";
    public const string AndroidGoogleAppId = "1:123456789012:android:0123456789abcdef";
    public const string IosGoogleAppId = "1:123456789012:ios:0123456789abcdef";

    public static IntegrationTestBackend Backend {
        get {
            var backend = GetConfigurationValue(
                "PLUGIN_FIREBASE_TEST_BACKEND",
                "debug.pluginfirebase.backend");

            if(string.IsNullOrWhiteSpace(backend)
                || string.Equals(backend, "emulator", StringComparison.OrdinalIgnoreCase)) {
                return IntegrationTestBackend.Emulator;
            }

            if(string.Equals(backend, "real", StringComparison.OrdinalIgnoreCase)) {
                return IntegrationTestBackend.Real;
            }

            throw new InvalidOperationException(
                $"PLUGIN_FIREBASE_TEST_BACKEND must be 'emulator' or 'real', but was '{backend}'.");
        }
    }

    public static bool UsesEmulatorBackend => Backend == IntegrationTestBackend.Emulator;

    public static bool UsesRealBackend => Backend == IntegrationTestBackend.Real;

    public static bool ShouldUseAuthEmulator => UsesEmulatorBackend || IsFeatureEnabled(
        "PLUGIN_FIREBASE_USE_AUTH_EMULATOR",
        "debug.pluginfirebase.auth.use");

    public static bool ShouldUseFirestoreEmulator => UsesEmulatorBackend || IsFeatureEnabled(
        "PLUGIN_FIREBASE_USE_FIRESTORE_EMULATOR",
        "debug.pluginfirebase.firestore.use");

    public static bool ShouldUseFunctionsEmulator => UsesEmulatorBackend || IsFeatureEnabled(
        "PLUGIN_FIREBASE_USE_FUNCTIONS_EMULATOR",
        "debug.pluginfirebase.functions.use");

    public static bool ShouldUseStorageEmulator => UsesEmulatorBackend || IsFeatureEnabled(
        "PLUGIN_FIREBASE_USE_STORAGE_EMULATOR",
        "debug.pluginfirebase.storage.use");

    public static EmulatorEndpoint AuthEmulatorEndpoint => GetEmulatorEndpoint(
        "PLUGIN_FIREBASE_AUTH_EMULATOR_HOST",
        "debug.pluginfirebase.auth.host",
        "PLUGIN_FIREBASE_AUTH_EMULATOR_PORT",
        "debug.pluginfirebase.auth.port",
        9099);

    public static EmulatorEndpoint FirestoreEmulatorEndpoint => GetEmulatorEndpoint(
        "PLUGIN_FIREBASE_FIRESTORE_EMULATOR_HOST",
        "debug.pluginfirebase.firestore.host",
        "PLUGIN_FIREBASE_FIRESTORE_EMULATOR_PORT",
        "debug.pluginfirebase.firestore.port",
        8080);

    public static EmulatorEndpoint FunctionsEmulatorEndpoint => GetEmulatorEndpoint(
        "PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST",
        "debug.pluginfirebase.functions.host",
        "PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT",
        "debug.pluginfirebase.functions.port",
        5001);

    public static EmulatorEndpoint StorageEmulatorEndpoint => GetEmulatorEndpoint(
        "PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST",
        "debug.pluginfirebase.storage.host",
        "PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT",
        "debug.pluginfirebase.storage.port",
        9199);

    public static void SkipIfEmulatorBackend(string reason)
    {
        if(UsesEmulatorBackend) {
            throw SkipException.ForSkip(reason);
        }
    }

    public static bool IsFeatureEnabled(
        string environmentVariableName,
        string androidSystemPropertyName)
    {
        return string.Equals(
            GetConfigurationValue(environmentVariableName, androidSystemPropertyName),
            "1",
            StringComparison.Ordinal);
    }

    public static string GetConfigurationValue(
        string environmentVariableName,
        string androidSystemPropertyName)
    {
        var environmentVariableValue = Environment.GetEnvironmentVariable(environmentVariableName);
        if(!string.IsNullOrWhiteSpace(environmentVariableValue)) {
            return environmentVariableValue;
        }

#if ANDROID
        return GetAndroidSystemProperty(androidSystemPropertyName);
#else
        return null;
#endif
    }

    private static EmulatorEndpoint GetEmulatorEndpoint(
        string hostEnvironmentVariableName,
        string hostAndroidSystemPropertyName,
        string portEnvironmentVariableName,
        string portAndroidSystemPropertyName,
        int defaultPort)
    {
        var host = GetConfigurationValue(
            hostEnvironmentVariableName,
            hostAndroidSystemPropertyName);
        var port = GetEmulatorPort(
            portEnvironmentVariableName,
            portAndroidSystemPropertyName,
            defaultPort);

        return new EmulatorEndpoint(
            string.IsNullOrWhiteSpace(host) ? GetDefaultEmulatorHost() : host,
            port);
    }

    private static string GetDefaultEmulatorHost()
    {
        return OperatingSystem.IsAndroid() ? "10.0.2.2" : "localhost";
    }

    private static int GetEmulatorPort(
        string environmentVariableName,
        string androidSystemPropertyName,
        int defaultPort)
    {
        var portValue = GetConfigurationValue(environmentVariableName, androidSystemPropertyName);
        if(string.IsNullOrWhiteSpace(portValue)) {
            return defaultPort;
        }

        if(!int.TryParse(portValue, out var port)) {
            throw new InvalidOperationException(
                $"{environmentVariableName} must be an integer, but was '{portValue}'.");
        }

        return port;
    }

#if ANDROID
    private static string GetAndroidSystemProperty(string propertyName)
    {
        IntPtr? propertyValuePointer = null;

        try {
            var systemPropertiesClass = AndroidRuntime.JNIEnv.FindClass("android/os/SystemProperties");
            var getMethodId = AndroidRuntime.JNIEnv.GetStaticMethodID(
                systemPropertiesClass,
                "get",
                "(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;");

            using var propertyNameValue = new Java.Lang.String(propertyName);
            using var defaultValue = new Java.Lang.String(string.Empty);
            propertyValuePointer = AndroidRuntime.JNIEnv.CallStaticObjectMethod(
                systemPropertiesClass,
                getMethodId,
                new AndroidRuntime.JValue(propertyNameValue),
                new AndroidRuntime.JValue(defaultValue));

            return AndroidRuntime.JNIEnv.GetString(
                propertyValuePointer.Value,
                AndroidRuntime.JniHandleOwnership.DoNotTransfer);
        } catch {
            return null;
        }
        finally {
            if(propertyValuePointer.HasValue && propertyValuePointer.Value != IntPtr.Zero) {
                AndroidRuntime.JNIEnv.DeleteLocalRef(propertyValuePointer.Value);
            }
        }
    }
#endif
}