using Firebase.Auth;

namespace Plugin.Firebase.Auth.Platforms.iOS.PhoneNumber;

public sealed class PhoneNumberAuth : NSObject, IAuthUIDelegate
{
    private UIViewController? _viewController;
    private string? _verificationId;

    public async Task VerifyPhoneNumberAsync(UIViewController viewController, string phoneNumber)
    {
        _viewController = viewController;
        _verificationId = await PhoneAuthProvider.DefaultInstance.VerifyPhoneNumberAsync(
            phoneNumber,
            this
        );
    }

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

    public void PresentViewController(
        UIViewController viewControllerToPresent,
        bool animated,
        Action? completion
    )
    {
        _viewController?.PresentViewController(viewControllerToPresent, animated, completion);
    }

    public void DismissViewController(bool animated, Action? completion)
    {
        _viewController?.DismissViewController(animated, completion);
    }
}