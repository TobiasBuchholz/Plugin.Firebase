namespace Plugin.Firebase.IntegrationTests;

internal static class IntegrationTestData
{
    public static string UniqueId(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}";
    }

    public static string UniqueEmail(string prefix)
    {
        return $"{UniqueId(prefix)}@test.com";
    }

    public static string UniqueFileName(string prefix, string extension)
    {
        return $"{UniqueId(prefix)}{extension}";
    }

    public static string GetRequiredConfigurationValue(
        string environmentVariableName,
        string? androidSystemPropertyName)
    {
        var value = IntegrationTestEnvironment.GetConfigurationValue(
            environmentVariableName,
            androidSystemPropertyName);
        if(string.IsNullOrWhiteSpace(value)) {
            throw new InvalidOperationException(
                $"Set {environmentVariableName} to run this opt-in acceptance test.");
        }

        return value;
    }
}