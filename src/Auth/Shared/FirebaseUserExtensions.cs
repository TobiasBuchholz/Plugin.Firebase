namespace Plugin.Firebase.Auth;

/// <summary>
/// Extension methods for <see cref="IFirebaseUser"/>.
/// </summary>
public static class FirebaseUserExtensions
{
    /// <summary>
    /// Changes the user's profile data.
    /// </summary>
    /// <param name="firebaseUser">The user to update.</param>
    /// <param name="request">The requested profile changes. Omitted fields remain unchanged.</param>
    public static Task UpdateProfileAsync(
        this IFirebaseUser firebaseUser,
        UserProfileChangeRequest request
    )
    {
        ArgumentNullException.ThrowIfNull(firebaseUser);

        if(firebaseUser is IUserProfileChangeRequestHandler handler) {
            return handler.UpdateProfileAsync(request);
        }

        if(
            (request.UpdatesDisplayName && request.DisplayName == "")
            || (request.UpdatesPhotoUrl && request.PhotoUrl == "")
        ) {
            throw new NotSupportedException(
                $"{nameof(UserProfileChangeRequest)} empty string values require an {nameof(IFirebaseUser)} implementation that supports the request-based profile update API."
            );
        }

#pragma warning disable CS0618
        if(request.UpdatesDisplayName && request.UpdatesPhotoUrl) {
            return firebaseUser.UpdateProfileAsync(request.DisplayName, request.PhotoUrl);
        }

        if(request.UpdatesDisplayName) {
            return firebaseUser.UpdateProfileAsync(displayName: request.DisplayName);
        }

        if(request.UpdatesPhotoUrl) {
            return firebaseUser.UpdateProfileAsync(photoUrl: request.PhotoUrl);
        }

        return firebaseUser.UpdateProfileAsync();
#pragma warning restore CS0618
    }
}

internal interface IUserProfileChangeRequestHandler
{
    Task UpdateProfileAsync(UserProfileChangeRequest request);
}