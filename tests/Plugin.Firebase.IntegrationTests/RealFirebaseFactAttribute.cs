namespace Plugin.Firebase.IntegrationTests;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class RealFirebaseFactAttribute : FactAttribute
{
    public RealFirebaseFactAttribute()
    {
        if(IntegrationTestEnvironment.UsesEmulatorBackend) {
            Skip = "This test requires a real Firebase project. Set PLUGIN_FIREBASE_TEST_BACKEND=real to run it.";
        }
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class RealFirebaseOptInFactAttribute : FactAttribute
{
    public RealFirebaseOptInFactAttribute(string environmentVariableName)
    {
        if(IntegrationTestEnvironment.UsesEmulatorBackend) {
            Skip = "This test requires a real Firebase project. Set PLUGIN_FIREBASE_TEST_BACKEND=real to run it.";
            return;
        }

        if(Environment.GetEnvironmentVariable(environmentVariableName) != "1") {
            Skip = $"Set {environmentVariableName}=1 to run this opt-in real Firebase test.";
        }
    }
}