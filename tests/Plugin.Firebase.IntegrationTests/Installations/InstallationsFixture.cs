using Plugin.Firebase.Installations;

namespace Plugin.Firebase.IntegrationTests.Installations
{
    [Collection("Sequential")]
    [TestLogging]
    [Preserve(AllMembers = true)]
    public class InstallationsFixture
    {
        private const string RunInstallationsDeleteTestsEnvironmentVariableName =
            "PLUGIN_FIREBASE_RUN_INSTALLATIONS_DELETE_TESTS";

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

        [RealFirebaseOptInFact(RunInstallationsDeleteTestsEnvironmentVariableName)]
        public async Task deletes_installation_when_enabled_via_environment()
        {
            var installationIdBeforeDelete = await CrossFirebaseInstallations.GetIdAsync();
            await CrossFirebaseInstallations.DeleteAsync();
            var installationIdAfterDelete = await CrossFirebaseInstallations.GetIdAsync();

            Assert.False(string.IsNullOrWhiteSpace(installationIdAfterDelete));
            Assert.NotEqual(installationIdBeforeDelete, installationIdAfterDelete);
        }
    }
}