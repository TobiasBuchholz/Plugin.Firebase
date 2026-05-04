using Plugin.Firebase.Auth;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.IntegrationTests.Auth
{
    public sealed partial class AuthFixture
    {
        [Fact]
        public async Task signs_out_user()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(
                sut,
                IntegrationTestUsers.SignOutEmail,
                deleteOnDispose: false);
            Assert.NotNull(sut.CurrentUser);

            await sut.SignOutAsync();
            Assert.Null(sut.CurrentUser);
        }

        [Fact]
        public async Task reloads_current_user()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(
                sut,
                IntegrationTestUsers.ReloadCurrentUserEmail,
                deleteOnDispose: false);
            Assert.NotNull(sut.CurrentUser);

            var uid = sut.CurrentUser!.Uid;
            await sut.CurrentUser!.ReloadAsync();

            Assert.NotNull(sut.CurrentUser);
            Assert.Equal(uid, sut.CurrentUser!.Uid);
        }

        [Fact]
        public async Task reloads_user()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("reload-user");
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(sut, email);
            Assert.NotNull(sut.CurrentUser);

            var uid = sut.CurrentUser!.Uid;
            await sut.CurrentUser!.ReloadAsync();

            Assert.NotNull(sut.CurrentUser);
            Assert.Equal(uid, sut.CurrentUser!.Uid);
        }

        [Fact]
        public async Task reload_current_user_fails_when_signed_out()
        {
            var sut = CrossFirebaseAuth.Current;
            await sut.SignOutAsync();

#pragma warning disable CS0618
            await Assert.ThrowsAnyAsync<FirebaseException>(sut.ReloadCurrentUserAsync);
#pragma warning restore CS0618
        }

        [Fact]
        public async Task deletes_user()
        {
            var sut = CrossFirebaseAuth.Current;
            var user = await sut.SignInWithEmailAndPasswordAsync(
                IntegrationTestUsers.DeleteUserEmail,
                IntegrationTestUsers.DefaultPassword);
            Assert.NotNull(sut.CurrentUser);

            await user.DeleteAsync();
            Assert.Null(sut.CurrentUser);
        }
    }
}