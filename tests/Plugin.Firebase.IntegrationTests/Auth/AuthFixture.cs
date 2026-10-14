using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Plugin.Firebase.Auth;

namespace Plugin.Firebase.IntegrationTests.Auth
{
    [Collection("Sequential")]
    [TestLogging]
    [IntegrationTestFixture(IntegrationTestPackage.Auth)]
    [Preserve(AllMembers = true)]
    public sealed partial class AuthFixture : IAsyncLifetime
    {
        private static readonly HttpClient HttpClient = new();

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            var sut = CrossFirebaseAuth.Current;
            if(sut.CurrentUser != null && IsEphemeralTestUser(sut.CurrentUser)) {
                try {
                    await sut.CurrentUser.DeleteAsync();
                } catch(Exception e) {
                    TestLog.Write($"[AUTH FIXTURE CLEANUP ERROR] {sut.CurrentUser?.Email ?? sut.CurrentUser?.Uid ?? "unknown"}: {e}");
                }
            }
            await sut.SignOutAsync();
        }

        private static bool IsEphemeralTestUser(IFirebaseUser user)
        {
            return user.IsAnonymous
                || IsUniqueTestEmail(user.Email);
        }

        private static bool IsUniqueTestEmail(string? email)
        {
            return email != null
                && Regex.IsMatch(email, "-[0-9a-fA-F]{32}@test\\.com$", RegexOptions.CultureInvariant);
        }

        private static ActionCodeSettings CreateActionCodeSettings()
        {
            var settings = new ActionCodeSettings {
                Url = "https://plugin.firebase.integrationtests/email-action",
                HandleCodeInApp = true,
                IOSBundleId = AppInfo.PackageName
            };
            settings.SetAndroidPackageName(AppInfo.PackageName, false, "1");
            return settings;
        }

        private static async Task<string> GetLatestAuthEmulatorEmailLinkAsync(
            string email,
            string requestType)
        {
            var endpoint = IntegrationTestEnvironment.AuthEmulatorEndpoint;
            var uri = $"http://{endpoint.Host}:{endpoint.Port}/emulator/v1/projects/{IntegrationTestEnvironment.ProjectId}/oobCodes";
            using var document = JsonDocument.Parse(await HttpClient.GetStringAsync(uri));

            var link = document.RootElement
                .GetProperty("oobCodes")
                .EnumerateArray()
                .Where(x =>
                    string.Equals(x.GetProperty("email").GetString(), email, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.GetProperty("requestType").GetString(), requestType, StringComparison.Ordinal))
                .Select(x => x.TryGetProperty("oobLink", out var oobLink) ? oobLink.GetString() : null)
                .LastOrDefault(x => !string.IsNullOrWhiteSpace(x));

            return link ?? throw new InvalidOperationException(
                $"Auth emulator did not expose a {requestType} email action link for {email}.");
        }

        private static async Task<bool> GetAuthEmulatorImprovedEmailPrivacyAsync()
        {
            using var document = JsonDocument.Parse(await HttpClient.GetStringAsync(GetAuthEmulatorConfigUri()));
            return ReadImprovedEmailPrivacy(document.RootElement);
        }

        // Toggles the Auth emulator's email enumeration protection, so sign-in reports
        // INVALID_LOGIN_CREDENTIALS for both a missing account and a wrong password.
        private static async Task SetAuthEmulatorImprovedEmailPrivacyAsync(bool enabled)
        {
            var body = new JsonObject {
                ["emailPrivacyConfig"] = new JsonObject { ["enableImprovedEmailPrivacy"] = enabled }
            };
            using var content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
            using var response = await HttpClient.PatchAsync(GetAuthEmulatorConfigUri(), content);
            response.EnsureSuccessStatusCode();

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if(ReadImprovedEmailPrivacy(document.RootElement) != enabled) {
                throw new InvalidOperationException(
                    $"Auth emulator did not apply enableImprovedEmailPrivacy={enabled}.");
            }
        }

        private static bool ReadImprovedEmailPrivacy(JsonElement config)
        {
            return config.TryGetProperty("emailPrivacyConfig", out var privacy)
                && privacy.TryGetProperty("enableImprovedEmailPrivacy", out var enabled)
                && enabled.GetBoolean();
        }

        private static string GetAuthEmulatorConfigUri()
        {
            var endpoint = IntegrationTestEnvironment.AuthEmulatorEndpoint;
            return $"http://{endpoint.Host}:{endpoint.Port}/emulator/v1/projects/{IntegrationTestEnvironment.ProjectId}/config";
        }

    }
}