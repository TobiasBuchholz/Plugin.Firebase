using Plugin.Firebase.Auth;

namespace Plugin.Firebase.IntegrationTests.Auth
{
    public sealed partial class AuthFixture
    {
        [Fact]
        public async Task sets_language_code()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(
                sut,
                IntegrationTestUsers.SetLanguageCodeEmail,
                deleteOnDispose: false);

            var ex = Record.Exception(() => {
                sut.LanguageCode = "fr";
                sut.UseAppLanguage();
            });
            Assert.Null(ex);
        }

        [Fact]
        public async Task exposes_user_metadata_and_provider_infos_after_sign_in()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("user-metadata");
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(sut, email);

            Assert.False(string.IsNullOrWhiteSpace(user.User.ProviderId));
            Assert.NotNull(user.User.ProviderInfos);
            Assert.Contains(user.User.ProviderInfos, x => x.Email == email);
            Assert.NotNull(user.User.Metadata);
            Assert.NotEqual(default, user.User.Metadata.CreationDate);
            Assert.NotEqual(default, user.User.Metadata.LastSignInDate);
        }
    }
}