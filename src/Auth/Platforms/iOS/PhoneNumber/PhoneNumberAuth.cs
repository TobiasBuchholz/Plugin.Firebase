using Firebase.Auth;

namespace Plugin.Firebase.Auth.Platforms.iOS.PhoneNumber;

/// <summary>
/// Provides phone number authentication functionality for iOS using Firebase Auth.
/// Implements IAuthUIDelegate to handle the reCAPTCHA verification UI.
/// </summary>
public sealed class PhoneNumberAuth : NSObject, IAuthUIDelegate
{
    private UIViewController? _viewController;
    private string? _verificationId;

    /// <summary>
    /// Initiates phone number verification by sending an SMS code to the specified phone number.
    /// </summary>
    /// <param name="viewController">The view controller to use for presenting the reCAPTCHA UI.</param>
    /// <param name="phoneNumber">The phone number to verify in E.164 format.</param>
    /// <returns>A task that completes when the verification SMS has been sent.</returns>
    public async Task VerifyPhoneNumberAsync(UIViewController viewController, string phoneNumber)
    {
        _viewController = viewController;
        _verificationId = await PhoneAuthProvider.DefaultInstance.VerifyPhoneNumberAsync(
            phoneNumber,
            this
        );
    }

    /// <summary>
    /// Gets a Firebase credential using the verification code received via SMS.
    /// </summary>
    /// <param name="verificationCode">The SMS verification code entered by the user.</param>
    /// <returns>A task containing the phone authentication credential.</returns>
    /// <exception cref="InvalidOperationException">Thrown when VerifyPhoneNumberAsync has not been called first.</exception>
    public Task<PhoneAuthCredential> GetCredentialAsync(string verificationCode)
    {
        if(_verificationId is null) {
            throw new InvalidOperationException(
                "VerifyPhoneNumberAsync must be called before GetCredentialAsync."
            );
        }
        return Task.FromResult(
            PhoneAuthProvider.DefaultInstance.GetCredential(_verificationId, verificationCode)
        );
    }

    /// <summary>
    /// Presents a view controller for the reCAPTCHA verification flow.
    /// </summary>
    /// <param name="viewControllerToPresent">The view controller to present.</param>
    /// <param name="animated">Whether to animate the presentation.</param>
    /// <param name="completion">Optional completion handler called after presentation.</param>
    public void PresentViewController(
        UIViewController viewControllerToPresent,
        bool animated,
        Action? completion
    )
    {
        _viewController?.PresentViewController(viewControllerToPresent, animated, completion);
    }

    /// <summary>
    /// Dismisses the currently presented view controller.
    /// </summary>
    /// <param name="animated">Whether to animate the dismissal.</param>
    /// <param name="completion">Optional completion handler called after dismissal.</param>
    public void DismissViewController(bool animated, Action? completion)
    {
        _viewController?.DismissViewController(animated, completion);
    }
}