using Firebase.Auth;
using Plugin.Firebase.Auth.Platforms.iOS.Extensions;
using Plugin.Firebase.Core.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Auth.Platforms.iOS;

/// <summary>
/// Wraps a native iOS Firebase AuthTokenResult for cross-platform access.
/// </summary>
public sealed class AuthTokenResultWrapper : IAuthTokenResult
{
    private readonly AuthTokenResult _wrapped;

    /// <summary>
    /// Initializes a new instance wrapping the specified native auth token result.
    /// </summary>
    /// <param name="wrapped">The native iOS Firebase AuthTokenResult to wrap.</param>
    public AuthTokenResultWrapper(AuthTokenResult wrapped)
    {
        _wrapped = wrapped;
    }

    /// <inheritdoc/>
    public T GetClaim<T>(string key)
    {
        return (T) _wrapped.Claims[key].ToObject(typeof(T))!;
    }

    /// <inheritdoc/>
    public DateTimeOffset AuthDate => _wrapped.AuthDate.ToDateTimeOffset();

    /// <inheritdoc/>
    public IDictionary<string, object> Claims => _wrapped.Claims.ToDictionary();

    /// <inheritdoc/>
    public DateTimeOffset ExpirationDate => _wrapped.ExpirationDate.ToDateTimeOffset();

    /// <inheritdoc/>
    public DateTimeOffset IssuedAtDate => _wrapped.IssuedAtDate.ToDateTimeOffset();

    /// <inheritdoc/>
    public string? SignInProvider => _wrapped.SignInProvider;

    /// <inheritdoc/>
    public string? SignInSecondFactor => _wrapped.SignInSecondFactor;

    /// <inheritdoc/>
    public string? Token => _wrapped.Token;
}