using Plugin.Firebase.Auth;

namespace Plugin.Firebase.IntegrationTests.Auth
{
    public sealed partial class AuthFixture
    {
        [Fact]
        public async Task updates_user_profile()
        {
            const string displayName = "Bruce Wayne";
            const string photoUrl = "https://url.to/image.jpg";
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithUniqueEmailAndPasswordAsync(
                sut,
                "update-profile");
            Assert.NotNull(sut.CurrentUser);
            Assert.Null(sut.CurrentUser!.DisplayName);
            Assert.Null(sut.CurrentUser!.PhotoUrl);

            await sut.CurrentUser!.UpdateProfileAsync(
                new UserProfileChangeRequest.Builder()
                    .SetDisplayName(displayName)
                    .SetPhotoUrl(photoUrl)
                    .Build()
            );
            Assert.Equal(displayName, sut.CurrentUser!.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser!.PhotoUrl);

            await sut.CurrentUser!.UpdateProfileAsync(
                new UserProfileChangeRequest.Builder()
                    .SetDisplayName(null)
                    .Build()
            );
            Assert.Null(sut.CurrentUser!.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser!.PhotoUrl);

            await sut.CurrentUser!.UpdateProfileAsync(
                new UserProfileChangeRequest.Builder()
                    .SetDisplayName(displayName)
                    .Build()
            );
            Assert.Equal(displayName, sut.CurrentUser!.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser!.PhotoUrl);

            await sut.CurrentUser!.UpdateProfileAsync(
                new UserProfileChangeRequest.Builder()
                    .SetPhotoUrl(null)
                    .Build()
            );
            Assert.Equal(displayName, sut.CurrentUser!.DisplayName);
            Assert.Null(sut.CurrentUser!.PhotoUrl);

            await sut.CurrentUser!.UpdateProfileAsync(
                new UserProfileChangeRequest.Builder()
                    .SetPhotoUrl(photoUrl)
                    .Build()
            );
            Assert.Equal(displayName, sut.CurrentUser!.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser!.PhotoUrl);

            await sut.CurrentUser!.UpdateProfileAsync(
                new UserProfileChangeRequest.Builder()
                    .SetDisplayName("")
                    .Build()
            );
            Assert.Equal("", sut.CurrentUser!.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser!.PhotoUrl);
        }

        [Fact]
        public async Task legacy_update_user_profile_overload_treats_empty_string_as_omitted()
        {
            const string displayName = "Bruce Wayne";
            const string photoUrl = "https://url.to/image.jpg";
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithUniqueEmailAndPasswordAsync(
                sut,
                "legacy-update-profile");
            Assert.NotNull(sut.CurrentUser);

#pragma warning disable CS0618
            await sut.CurrentUser!.UpdateProfileAsync(displayName, photoUrl);
            await sut.CurrentUser!.UpdateProfileAsync(displayName: "");
            await sut.CurrentUser!.UpdateProfileAsync(photoUrl: "");
            Assert.Equal(displayName, sut.CurrentUser!.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser!.PhotoUrl);

            await sut.CurrentUser!.UpdateProfileAsync(null);
            Assert.Null(sut.CurrentUser!.DisplayName);
            Assert.Equal(photoUrl, sut.CurrentUser!.PhotoUrl);
#pragma warning restore CS0618
        }
    }
}