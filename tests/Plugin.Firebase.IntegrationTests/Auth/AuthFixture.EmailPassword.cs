using Plugin.Firebase.Auth;

namespace Plugin.Firebase.IntegrationTests.Auth
{
    public sealed partial class AuthFixture
    {
        // Firebase now requires verify-before-update for newer projects on iOS and Android,
        // so this direct email update path only works with deprecated project configuration.
        [Fact(Skip = "Firebase direct email updates on iOS and Android rely on deprecated project configuration.")]
        public async Task updates_user_email()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(
                sut,
                IntegrationTestUsers.UpdateEmailEmail,
                deleteOnDispose: false);
            Assert.NotNull(sut.CurrentUser);

            await sut.CurrentUser!.UpdateEmailAsync("updated@test.com");
            Assert.Equal("updated@test.com", sut.CurrentUser!.Email);
        }

        [Fact]
        public async Task updates_user_password()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("update-password");
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(sut, email);
            Assert.NotNull(sut.CurrentUser);

            await sut.CurrentUser!.UpdatePasswordAsync(IntegrationTestUsers.UpdatedPassword);
            await sut.SignOutAsync();
            Assert.Null(sut.CurrentUser);

            await Assert.ThrowsAnyAsync<Exception>(
                () => sut.SignInWithEmailAndPasswordAsync(email, IntegrationTestUsers.DefaultPassword));
            await sut.SignInWithEmailAndPasswordAsync(email, IntegrationTestUsers.UpdatedPassword);
            Assert.NotNull(sut.CurrentUser);
        }
    }
}