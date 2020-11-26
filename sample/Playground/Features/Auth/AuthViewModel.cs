using Playground.Common.Base;
using Plugin.Firebase.Auth;

namespace Playground.Features.Auth
{
    public sealed class AuthViewModel : ViewModelBase
    {
        private readonly IFirebaseAuth _firebaseAuth;
        
        public AuthViewModel(IFirebaseAuth firebaseAuth)
        {
            _firebaseAuth = firebaseAuth;
        }
    }
}