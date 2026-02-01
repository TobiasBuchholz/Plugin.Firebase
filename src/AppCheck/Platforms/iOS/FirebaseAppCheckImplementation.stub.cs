#if !FIREBASE_APP_CHECK_IOS
namespace Plugin.Firebase.AppCheck;

/// <summary>
/// iOS stub implementation of <see cref="IFirebaseAppCheck"/> for platforms where AppCheck is not enabled.
/// </summary>
public sealed class FirebaseAppCheckImplementation : IFirebaseAppCheck
{
    /// <summary>
    /// Configures Firebase AppCheck with the specified options.
    /// </summary>
    /// <param name="options">The AppCheck options to apply.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    /// <exception cref="NotSupportedException">Thrown if AppCheck mode is not Disabled.</exception>
    public void Configure(AppCheckOptions options)
    {
        if(options == null) {
            throw new ArgumentNullException(nameof(options));
        }

        if(options.Mode != AppCheckMode.Disabled) {
            throw new NotSupportedException("Firebase App Check for iOS is not enabled. Set EnableFirebaseAppCheckIos=true once AdamE.Firebase.iOS.AppCheck (12.5.0.4+) is available.");
        }
    }

    /// <summary>
    /// Disposes resources used by the implementation.
    /// </summary>
    public void Dispose()
    {
    }
}
#endif