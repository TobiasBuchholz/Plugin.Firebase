using System;
using System.Collections.Generic;
using Firebase.Auth;
using Plugin.Firebase.Auth;
using Plugin.Firebase.iOS.Extensions;

namespace Plugin.Firebase.iOS.Auth
{
    public sealed class AuthTokenResultWrapper : IAuthTokenResult
    {
        private readonly AuthTokenResult _wrapped;

        public AuthTokenResultWrapper(AuthTokenResult wrapped)
        {
            _wrapped = wrapped;
        }

        public T GetClaim<T>(string key)
        {
            return (T) _wrapped.Claims[key].ToObject(typeof(T));
        }

        public DateTimeOffset AuthDate => _wrapped.AuthDate.ToDateTimeOffset();
        public IDictionary<string, object> Claims => _wrapped.Claims.ToDictionary();
        public DateTimeOffset ExpirationDate => _wrapped.ExpirationDate.ToDateTimeOffset();
        public DateTimeOffset IssuedAtDate => _wrapped.IssuedAtDate.ToDateTimeOffset();
        public string SignInProvider => _wrapped.SignInProvider;
        public string SignInSecondFactor => _wrapped.SignInSecondFactor;
        public string Token => _wrapped.Token;
    }
}