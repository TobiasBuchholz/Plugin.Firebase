using System;
using System.Threading.Tasks;
using Android.Runtime;
using Plugin.Firebase.Auth;
using Xunit;

namespace Plugin.Firebase.IntegrationTests.Auth
{
    [Collection("Sequential")]
    [Preserve(AllMembers = true)]
    public sealed class AuthFixture : IAsyncLifetime
    {
        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }
        
        [Fact]
        public async Task signs_in_user_via_email_and_password()
        {
            var sut = CrossFirebaseAuth.Current;
            var user = await sut.SignInWithEmailAndPasswordAsync("sign-in-with-pw@test.com", "123456");
            Assert.Equal("sign-in-with-pw@test.com", user.Email);
            Assert.Equal("sign-in-with-pw@test.com", sut.CurrentUser.Email);
        }

        [Fact]
        public async Task signs_in_user_anonymously()
        {
            var sut = CrossFirebaseAuth.Current;
            Assert.Null(sut.CurrentUser);

            var user = await sut.SignInAnonymouslyAsync();
            Assert.NotNull(user);
            Assert.NotNull(sut.CurrentUser);
            Assert.True(user.IsAnonymous);
        }

        [Fact]
        public async Task signs_out_user()
        {
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync("sign-out@test.com", "123456");
            Assert.NotNull(sut.CurrentUser);

            await sut.SignOutAsync();
            Assert.Null(sut.CurrentUser);
        }

        [Fact]
        public async Task updates_user_email()
        {
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync("to-update-email@test.com", "123456");
            Assert.NotNull(sut.CurrentUser);

            await sut.CurrentUser.UpdateEmailAsync("updated@test.com");
            Assert.Equal("updated@test.com", sut.CurrentUser.Email);
        }

        [Fact]
        public async Task updates_user_password()
        {
            const string email = "to-update-pw@test.com";
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync(email, "123456");
            Assert.NotNull(sut.CurrentUser);
            
            await sut.CurrentUser.UpdatePasswordAsync("abcdefgh");
            await sut.SignOutAsync();
            Assert.Null(sut.CurrentUser);

            await Assert.ThrowsAnyAsync<Exception>(() => sut.SignInWithEmailAndPasswordAsync(email, "123456"));
            await sut.SignInWithEmailAndPasswordAsync(email, "abcdefgh");
            Assert.NotNull(sut.CurrentUser);
        }

        [Fact]
        public async Task updates_user_profile()
        {
            const string displayName = "Bruce Wayne";
            const string photoUrl = "https://url.to/image.jpg";
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync("to-update-profile@test.com", "123456");
            Assert.NotNull(sut.CurrentUser);
            Assert.Null(sut.CurrentUser.DisplayName);
            Assert.Null(sut.CurrentUser.PhotoUrl);

            await sut.CurrentUser.UpdateProfileAsync(displayName, photoUrl);
            Assert.Equal(displayName, sut.CurrentUser.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser.PhotoUrl);
        }

        [Fact]
        public async Task sends_verification_email()
        {
            var sut = CrossFirebaseAuth.Current;
            await sut.SignInWithEmailAndPasswordAsync("verification-email@test.com", "123456");
            Assert.NotNull(sut.CurrentUser);

            await sut.CurrentUser.SendEmailVerificationAsync();
        }
        
        [Fact]
        public async Task deletes_user()
        {
            var sut = CrossFirebaseAuth.Current;
            var user = await sut.SignInWithEmailAndPasswordAsync("to-delete@test.com", "123456");
            Assert.NotNull(sut.CurrentUser);

            await user.DeleteAsync();
            Assert.Null(sut.CurrentUser);
        }

        public async Task DisposeAsync()
        {
            var sut = CrossFirebaseAuth.Current;
            if(sut.CurrentUser != null) {
                await sut.CurrentUser.DeleteAsync();
            }
            await sut.SignOutAsync();
        }
    }
}