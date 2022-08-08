using System;
using System.Collections.Generic;
using Firebase.Auth;
using Plugin.Firebase.Android.Extensions;
using Plugin.Firebase.Auth;

namespace Plugin.Firebase.Android.Auth
{
    public sealed class AuthTokenResultWrapper : IAuthTokenResult
    {
        private readonly GetTokenResult _wrapped;

        public AuthTokenResultWrapper(GetTokenResult wrapped)
        {
            _wrapped = wrapped;
        }

        public T GetClaim<T>(string key)
        {
            return (T) _wrapped.Claims[key].ToObject(typeof(T));
        }

        public DateTimeOffset AuthDate => DateTimeOffset.FromUnixTimeMilliseconds(_wrapped.AuthTimestamp);
        public IDictionary<string, object> Claims => _wrapped.Claims.ToDictionary();
        public DateTimeOffset ExpirationDate => DateTimeOffset.FromUnixTimeMilliseconds(_wrapped.ExpirationTimestamp);
        public DateTimeOffset IssuedAtDate => DateTimeOffset.FromUnixTimeMilliseconds(_wrapped.IssuedAtTimestamp);
        public string SignInProvider => _wrapped.SignInProvider;
        public string SignInSecondFactor => _wrapped.SignInSecondFactor;
        public string Token => _wrapped.Token;
    }
}