namespace Plugin.Firebase.IntegrationTests;

internal enum IntegrationTestBackend
{
    Emulator,
    Real
}

internal readonly record struct EmulatorEndpoint(string Host, int Port);

internal static partial class IntegrationTestEnvironment
{
    public const string ProjectId = "demo-pluginfirebase-integrationtests";
    public const string StorageBucket = "demo-pluginfirebase-integrationtests.appspot.com";
    public const string DatabaseUrl = "https://demo-pluginfirebase-integrationtests.firebaseio.com";
    public const string ApiKey = "AIzaSyD00000000000000000000000000000000";
    public const string GcmSenderId = "123456789012";
    public const string AndroidGoogleAppId = "1:123456789012:android:0123456789abcdef";
    public const string IosGoogleAppId = "1:123456789012:ios:0123456789abcdef";
    public const string RunAppCheckTokenTestsEnvironmentVariableName =
        IntegrationTestOptions.RunAppCheckTokenTestsEnvironmentVariableName;

    public static IntegrationTestBackend Backend {
        get {
            var backend = GetConfigurationValue(
                IntegrationTestOptions.BackendEnvironmentVariableName,
                IntegrationTestOptions.BackendAndroidSystemPropertyName);

            if(string.IsNullOrWhiteSpace(backend)
                || string.Equals(backend, "emulator", StringComparison.OrdinalIgnoreCase)) {
                return IntegrationTestBackend.Emulator;
            }

            if(string.Equals(backend, "real", StringComparison.OrdinalIgnoreCase)) {
                return IntegrationTestBackend.Real;
            }

            throw new InvalidOperationException(
                $"{IntegrationTestOptions.BackendEnvironmentVariableName} must be 'emulator' or 'real', but was '{backend}'.");
        }
    }

    public static bool UsesEmulatorBackend => Backend == IntegrationTestBackend.Emulator;

    public static bool UsesRealBackend => Backend == IntegrationTestBackend.Real;

    public static bool ShouldRunAppCheckTokenTests =>
        Environment.GetEnvironmentVariable(IntegrationTestOptions.RunAppCheckTokenTestsEnvironmentVariableName) == "1";

    public static bool ShouldUseAuthEmulator => UsesEmulatorBackend || IsFeatureEnabled(
        IntegrationTestOptions.UseAuthEmulatorEnvironmentVariableName,
        IntegrationTestOptions.UseAuthEmulatorAndroidSystemPropertyName);

    public static bool ShouldUseFirestoreEmulator => UsesEmulatorBackend || IsFeatureEnabled(
        IntegrationTestOptions.UseFirestoreEmulatorEnvironmentVariableName,
        IntegrationTestOptions.UseFirestoreEmulatorAndroidSystemPropertyName);

    public static bool ShouldUseFunctionsEmulator => UsesEmulatorBackend || IsFeatureEnabled(
        IntegrationTestOptions.UseFunctionsEmulatorEnvironmentVariableName,
        IntegrationTestOptions.UseFunctionsEmulatorAndroidSystemPropertyName);

    public static bool ShouldUseStorageEmulator => UsesEmulatorBackend || IsFeatureEnabled(
        IntegrationTestOptions.UseStorageEmulatorEnvironmentVariableName,
        IntegrationTestOptions.UseStorageEmulatorAndroidSystemPropertyName);

    public static EmulatorEndpoint AuthEmulatorEndpoint => GetEmulatorEndpoint(
        IntegrationTestOptions.AuthEmulatorHostEnvironmentVariableName,
        IntegrationTestOptions.AuthEmulatorHostAndroidSystemPropertyName,
        IntegrationTestOptions.AuthEmulatorPortEnvironmentVariableName,
        IntegrationTestOptions.AuthEmulatorPortAndroidSystemPropertyName,
        9099);

    public static EmulatorEndpoint FirestoreEmulatorEndpoint => GetEmulatorEndpoint(
        IntegrationTestOptions.FirestoreEmulatorHostEnvironmentVariableName,
        IntegrationTestOptions.FirestoreEmulatorHostAndroidSystemPropertyName,
        IntegrationTestOptions.FirestoreEmulatorPortEnvironmentVariableName,
        IntegrationTestOptions.FirestoreEmulatorPortAndroidSystemPropertyName,
        8080);

    public static EmulatorEndpoint FunctionsEmulatorEndpoint => GetEmulatorEndpoint(
        IntegrationTestOptions.FunctionsEmulatorHostEnvironmentVariableName,
        IntegrationTestOptions.FunctionsEmulatorHostAndroidSystemPropertyName,
        IntegrationTestOptions.FunctionsEmulatorPortEnvironmentVariableName,
        IntegrationTestOptions.FunctionsEmulatorPortAndroidSystemPropertyName,
        5001);

    public static EmulatorEndpoint StorageEmulatorEndpoint => GetEmulatorEndpoint(
        IntegrationTestOptions.StorageEmulatorHostEnvironmentVariableName,
        IntegrationTestOptions.StorageEmulatorHostAndroidSystemPropertyName,
        IntegrationTestOptions.StorageEmulatorPortEnvironmentVariableName,
        IntegrationTestOptions.StorageEmulatorPortAndroidSystemPropertyName,
        9199);

    public static bool IsFeatureEnabled(
        string environmentVariableName,
        string? androidSystemPropertyName)
    {
        return string.Equals(
            GetConfigurationValue(environmentVariableName, androidSystemPropertyName),
            "1",
            StringComparison.Ordinal);
    }

    public static string? GetConfigurationValue(
        string environmentVariableName,
        string? androidSystemPropertyName)
    {
        var environmentVariableValue = Environment.GetEnvironmentVariable(environmentVariableName);
        return !string.IsNullOrWhiteSpace(environmentVariableValue)
            ? environmentVariableValue
            : GetPlatformConfigurationValue(androidSystemPropertyName);
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

    private static partial string? GetPlatformConfigurationValue(string? androidSystemPropertyName);
}