using System;
using System.Collections.Generic;

namespace Plugin.Firebase.Auth
{
    /// <summary>
    /// A data class containing the ID token JWT string and other properties associated with the token including the decoded payload claims.
    /// </summary>
    public interface IAuthTokenResult
    {
        /// <summary>
        /// Retrieves the claim casted to the generic type param.
        /// </summary>
        /// <param name="key">Key of the claim</param>
        /// <typeparam name="T">Type of the claim</typeparam>
        /// <returns></returns>
        T GetClaim<T>(string key);
        
        /// <summary>
        /// Stores the ID token’s authentication date. This is the date the user was signed in and NOT the date the token was refreshed.
        /// </summary>
        DateTimeOffset AuthDate { get; }
        
        
        /// <summary>
        /// Stores the entire payload of claims found on the ID token. This includes the standard reserved claims as well as custom claims
        /// set by the developer via the Admin SDK.
        /// </summary>
        IDictionary<string, object> Claims { get; }
        
        /// <summary>
        /// Stores the ID token’s expiration date.
        /// </summary>
        DateTimeOffset ExpirationDate { get; }
        
        /// <summary>
        /// Stores the date that the ID token was issued. This is the date last refreshed and NOT the last authentication date.
        /// </summary>
        DateTimeOffset IssuedAtDate { get; }
        
        /// <summary>
        /// Stores sign-in provider through which the token was obtained. This does not necessarily map to provider IDs.
        /// </summary>
        string SignInProvider { get; }
        
        /// <summary>
        /// Stores sign-in second factor through which the token was obtained.
        /// </summary>
        string SignInSecondFactor { get; }
        
        /// <summary>
        /// Stores the JWT string of the ID token.
        /// </summary>
        string Token { get; }
    }
}