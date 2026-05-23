using Plugin.Firebase.Installations;

namespace Plugin.Firebase.IntegrationTests.Installations;

[Collection("Sequential")]
[TestLogging]
[IntegrationTestFixture(IntegrationTestPackage.Installations)]
[Preserve(AllMembers = true)]
public class InstallationsFixture
{
    [Fact]
    public void disposes_and_reacquires_installations_singleton()
    {
        var first = CrossFirebaseInstallations.Current;

        CrossFirebaseInstallations.Dispose();

        var second = CrossFirebaseInstallations.Current;
        Assert.NotNull(second);
        Assert.NotSame(first, second);
    }

    [RealFirebaseFact]
    public async Task gets_stable_installation_id()
    {
        var firstInstallationId = await CrossFirebaseInstallations.GetIdAsync();
        var secondInstallationId = await CrossFirebaseInstallations.GetIdAsync();

        Assert.False(string.IsNullOrWhiteSpace(firstInstallationId));
        Assert.Equal(firstInstallationId, secondInstallationId);
    }

    [RealFirebaseFact]
    public async Task gets_installation_tokens()
    {
        var token = await CrossFirebaseInstallations.GetTokenAsync();
        var refreshedToken = await CrossFirebaseInstallations.GetTokenAsync(forceRefresh: true);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.False(string.IsNullOrWhiteSpace(refreshedToken));
    }

    [RealFirebaseOptInFact(IntegrationTestOptions.RunInstallationsDeleteTestsEnvironmentVariableName)]
    public async Task deletes_installation_when_enabled_via_environment()
    {
        var installationIdBeforeDelete = await CrossFirebaseInstallations.GetIdAsync();
        await CrossFirebaseInstallations.DeleteAsync();
        var installationIdAfterDelete = await CrossFirebaseInstallations.GetIdAsync();

        Assert.False(string.IsNullOrWhiteSpace(installationIdAfterDelete));
        Assert.NotEqual(installationIdBeforeDelete, installationIdAfterDelete);
    }
}