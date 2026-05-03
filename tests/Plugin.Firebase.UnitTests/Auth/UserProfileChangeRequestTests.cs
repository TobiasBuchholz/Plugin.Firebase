using Plugin.Firebase.Auth;

namespace Plugin.Firebase.UnitTests.Auth;

public class UserProfileChangeRequestTests
{
    [Fact]
    public void build_without_values_marks_no_fields_for_update()
    {
        var sut = new UserProfileChangeRequest.Builder().Build();

        Assert.False(sut.UpdatesDisplayName);
        Assert.Null(sut.DisplayName);
        Assert.False(sut.UpdatesPhotoUrl);
        Assert.Null(sut.PhotoUrl);
    }

    [Fact]
    public void set_display_name_tracks_null_value()
    {
        var sut = new UserProfileChangeRequest.Builder()
            .SetDisplayName(null)
            .Build();

        Assert.True(sut.UpdatesDisplayName);
        Assert.Null(sut.DisplayName);
        Assert.False(sut.UpdatesPhotoUrl);
    }

    [Fact]
    public void set_photo_url_tracks_empty_string_value()
    {
        var sut = new UserProfileChangeRequest.Builder()
            .SetPhotoUrl("")
            .Build();

        Assert.False(sut.UpdatesDisplayName);
        Assert.True(sut.UpdatesPhotoUrl);
        Assert.Equal("", sut.PhotoUrl);
    }

    [Fact]
    public void build_captures_current_builder_values()
    {
        var builder = new UserProfileChangeRequest.Builder()
            .SetDisplayName("Ada")
            .SetPhotoUrl("https://url.to/ada.jpg");

        var sut = builder.Build();
        builder.SetDisplayName("Grace").SetPhotoUrl(null);

        Assert.True(sut.UpdatesDisplayName);
        Assert.Equal("Ada", sut.DisplayName);
        Assert.True(sut.UpdatesPhotoUrl);
        Assert.Equal("https://url.to/ada.jpg", sut.PhotoUrl);
    }

    [Fact]
    public async Task extension_delegates_to_legacy_overload_for_existing_implementations()
    {
        IFirebaseUser sut = new LegacyFirebaseUser();
        var request = new UserProfileChangeRequest.Builder()
            .SetDisplayName(null)
            .SetPhotoUrl("https://url.to/ada.jpg")
            .Build();

        await sut.UpdateProfileAsync(request);

        var legacyUser = Assert.IsType<LegacyFirebaseUser>(sut);
        Assert.Null(legacyUser.DisplayNameArgument);
        Assert.Equal("https://url.to/ada.jpg", legacyUser.PhotoUrlArgument);
    }

    [Fact]
    public async Task extension_rejects_empty_strings_for_existing_implementations()
    {
        IFirebaseUser sut = new LegacyFirebaseUser();
        var request = new UserProfileChangeRequest.Builder()
            .SetDisplayName("")
            .Build();

        await Assert.ThrowsAsync<NotSupportedException>(() => sut.UpdateProfileAsync(request));
    }

    private sealed class LegacyFirebaseUser : IFirebaseUser
    {
        public string? DisplayNameArgument { get; private set; }
        public string? PhotoUrlArgument { get; private set; }

        public Task UpdateEmailAsync(string email) => Task.CompletedTask;

        public Task UpdatePasswordAsync(string password) => Task.CompletedTask;

        public Task ReauthenticateWithEmailAndPasswordAsync(string email, string password) =>
            Task.CompletedTask;

        public Task ReloadAsync() => Task.CompletedTask;

        public Task UpdatePhoneNumberAsync(string verificationId, string smsCode) => Task.CompletedTask;

#pragma warning disable CS0618
        public Task UpdateProfileAsync(string? displayName = "", string? photoUrl = "")
#pragma warning restore CS0618
        {
            DisplayNameArgument = displayName;
            PhotoUrlArgument = photoUrl;
            return Task.CompletedTask;
        }

        public Task SendEmailVerificationAsync(ActionCodeSettings? actionCodeSettings = null) =>
            Task.CompletedTask;

        public Task UnlinkAsync(string providerId) => Task.CompletedTask;

        public Task DeleteAsync() => Task.CompletedTask;

        public Task<IAuthTokenResult> GetIdTokenResultAsync(bool forceRefresh = false) =>
            throw new NotImplementedException();

        public string Uid => "uid";
        public string? DisplayName => null;
        public string? Email => null;
        public string? PhotoUrl => null;
        public string ProviderId => "firebase";
        public bool IsEmailVerified => false;
        public bool IsAnonymous => false;
        public IEnumerable<ProviderInfo>? ProviderInfos => null;
        public UserMetadata? Metadata => null;
    }
}