namespace Plugin.Firebase.IntegrationTests;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class EmulatorBackendFactAttribute : FactAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.Emulator;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.Any;

    public string? OptIn => null;

    public EmulatorBackendFactAttribute()
    {
        Skip = IntegrationTestSkipPolicy.RequireEmulatorBackend();
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class EmulatorBackendTheoryAttribute : TheoryAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.Emulator;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.Any;

    public string? OptIn => null;

    public EmulatorBackendTheoryAttribute()
    {
        Skip = IntegrationTestSkipPolicy.RequireEmulatorBackend();
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class RealFirebaseFactAttribute : FactAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.Real;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.Any;

    public string? OptIn => null;

    public RealFirebaseFactAttribute()
    {
        Skip = IntegrationTestSkipPolicy.RequireRealBackend();
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class RealFirebaseTheoryAttribute : TheoryAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.Real;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.Any;

    public string? OptIn => null;

    public RealFirebaseTheoryAttribute()
    {
        Skip = IntegrationTestSkipPolicy.RequireRealBackend();
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class RealFirebaseOptInFactAttribute : FactAttribute, IIntegrationTestCaseMetadata
{
    private readonly bool _skipIosSimulator;

    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.RealOptIn;

    public IntegrationTestPlatformRequirement Platform =>
        _skipIosSimulator ? IntegrationTestPlatformRequirement.IosDevice : IntegrationTestPlatformRequirement.Any;

    public string? OptIn { get; }

    public RealFirebaseOptInFactAttribute(
        string environmentVariableName,
        string? androidSystemPropertyName = null,
        bool skipIosSimulator = false)
    {
        _skipIosSimulator = skipIosSimulator;
        OptIn = environmentVariableName;
        Skip = IntegrationTestSkipPolicy.RequireRealBackendOptIn(
            environmentVariableName,
            androidSystemPropertyName,
            skipIosSimulator);
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class RealFirebaseOptInTheoryAttribute : TheoryAttribute, IIntegrationTestCaseMetadata
{
    private readonly bool _skipIosSimulator;

    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.RealOptIn;

    public IntegrationTestPlatformRequirement Platform =>
        _skipIosSimulator ? IntegrationTestPlatformRequirement.IosDevice : IntegrationTestPlatformRequirement.Any;

    public string? OptIn { get; }

    public RealFirebaseOptInTheoryAttribute(
        string environmentVariableName,
        string? androidSystemPropertyName = null,
        bool skipIosSimulator = false)
    {
        _skipIosSimulator = skipIosSimulator;
        OptIn = environmentVariableName;
        Skip = IntegrationTestSkipPolicy.RequireRealBackendOptIn(
            environmentVariableName,
            androidSystemPropertyName,
            skipIosSimulator);
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class OptInFactAttribute : FactAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.OptIn;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.Any;

    public string? OptIn { get; }

    public OptInFactAttribute(
        string environmentVariableName,
        string? androidSystemPropertyName = null)
    {
        OptIn = environmentVariableName;
        Skip = IntegrationTestSkipPolicy.RequireOptIn(environmentVariableName, androidSystemPropertyName);
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class OptInTheoryAttribute : TheoryAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.OptIn;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.Any;

    public string? OptIn { get; }

    public OptInTheoryAttribute(
        string environmentVariableName,
        string? androidSystemPropertyName = null)
    {
        OptIn = environmentVariableName;
        Skip = IntegrationTestSkipPolicy.RequireOptIn(environmentVariableName, androidSystemPropertyName);
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class AndroidFactAttribute : FactAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.Any;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.Android;

    public string? OptIn => null;

    public AndroidFactAttribute()
    {
        Skip = IntegrationTestSkipPolicy.RequireAndroid();
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class AndroidTheoryAttribute : TheoryAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.Any;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.Android;

    public string? OptIn => null;

    public AndroidTheoryAttribute()
    {
        Skip = IntegrationTestSkipPolicy.RequireAndroid();
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class IosFactAttribute : FactAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.Any;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.Ios;

    public string? OptIn => null;

    public IosFactAttribute()
    {
        Skip = IntegrationTestSkipPolicy.RequireIos();
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class IosTheoryAttribute : TheoryAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.Any;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.Ios;

    public string? OptIn => null;

    public IosTheoryAttribute()
    {
        Skip = IntegrationTestSkipPolicy.RequireIos();
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class IosDeviceFactAttribute : FactAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.Any;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.IosDevice;

    public string? OptIn => null;

    public IosDeviceFactAttribute()
    {
        Skip = IntegrationTestSkipPolicy.RequireIosDevice();
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class NonIosSimulatorFactAttribute : FactAttribute, IIntegrationTestCaseMetadata
{
    public IntegrationTestBackendRequirement Backend => IntegrationTestBackendRequirement.Any;

    public IntegrationTestPlatformRequirement Platform => IntegrationTestPlatformRequirement.NonIosSimulator;

    public string? OptIn => null;

    public NonIosSimulatorFactAttribute()
    {
        Skip = IntegrationTestSkipPolicy.RequireNonIosSimulator();
    }
}