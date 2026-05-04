namespace Plugin.Firebase.IntegrationTests;

internal enum IntegrationTestPackage
{
    Analytics,
    AppCheck,
    Auth,
    Bundled,
    CloudMessaging,
    Crashlytics,
    Firestore,
    Functions,
    Installations,
    PerformanceMonitoring,
    RemoteConfig,
    Storage
}

internal enum IntegrationTestBackendRequirement
{
    Any,
    Emulator,
    Real,
    RealOptIn,
    OptIn
}

internal enum IntegrationTestPlatformRequirement
{
    Any,
    Android,
    Ios,
    IosDevice,
    NonIosSimulator
}

internal interface IIntegrationTestCaseMetadata
{
    IntegrationTestBackendRequirement Backend { get; }

    IntegrationTestPlatformRequirement Platform { get; }

    string? OptIn { get; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class IntegrationTestFixtureAttribute(IntegrationTestPackage package) : Attribute
{
    public IntegrationTestPackage Package { get; } = package;
}

[AttributeUsage(AttributeTargets.Class)]
internal sealed class IntegrationTestCoverageIgnoreAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class IntegrationTestCaseAttribute(
    IntegrationTestBackendRequirement backend = IntegrationTestBackendRequirement.Any,
    IntegrationTestPlatformRequirement platform = IntegrationTestPlatformRequirement.Any,
    string? optIn = null) : Attribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend { get; } = backend;

    public IntegrationTestPlatformRequirement Platform { get; } = platform;

    public string? OptIn { get; } = optIn;
}