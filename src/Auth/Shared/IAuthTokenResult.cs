namespace Plugin.Firebase.Auth;

/// <summary>
/// A data class containing the ID token JWT string and other properties associated with the token including the decoded payload claims.
/// </summary>
public interface IAuthTokenResult
{
    /// <summary>
    /// Retrieves the claim converted to the generic type parameter. Nested JSON objects are returned as dictionaries, and
    /// nested JSON arrays are returned as lists.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A numeric claim can be read as any numeric type (<see cref="sbyte"/> through <see cref="ulong"/>,
    /// <see cref="float"/>, <see cref="double"/> and <see cref="decimal"/>), as an enum, or as a nullable form of
    /// either.
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// A fractional value read as an integral type or an enum truncates toward zero: 2.7 becomes 2 and -2.7 becomes -2.
    /// </description></item>
    /// <item><description>
    /// A value outside the range of the requested type throws <see cref="OverflowException"/> instead of wrapping. An
    /// enum is range checked against its underlying type, not against its defined members.
    /// </description></item>
    /// <item><description>
    /// NaN and infinity throw <see cref="OverflowException"/> when read as an integral type, an enum or
    /// <see cref="decimal"/>. A finite value beyond the range of <see cref="float"/> throws
    /// <see cref="OverflowException"/> instead of becoming infinity.
    /// </description></item>
    /// <item><description>
    /// Claims are not coerced between JSON types. Reading a string or boolean claim as a numeric type or an enum, a
    /// numeric claim as a <see cref="string"/> or <see cref="bool"/>, or any claim as <see cref="char"/> throws
    /// <see cref="InvalidCastException"/>.
    /// </description></item>
    /// </list>
    /// <para>
    /// If the token has no claim named <paramref name="key"/>, the call throws <see cref="KeyNotFoundException"/> on
    /// Android and <see cref="NullReferenceException"/> on iOS.
    /// </para>
    /// </remarks>
    /// <param name="key">Key of the claim</param>
    /// <typeparam name="T">Type of the claim</typeparam>
    /// <returns>The claim value converted to <typeparamref name="T"/>.</returns>
    /// <exception cref="OverflowException">
    /// The claim is numeric and its value doesn't fit <typeparamref name="T"/>.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// The claim's JSON type doesn't match <typeparamref name="T"/>.
    /// </exception>
    T GetClaim<T>(string key);

    /// <summary>
    /// Stores the ID token’s authentication date. This is the date the user was signed in and NOT the date the token was refreshed.
    /// </summary>
    DateTimeOffset AuthDate { get; }

    /// <summary>
    /// Stores the entire payload of claims found on the ID token. This includes the standard reserved claims as well as custom claims
    /// set by the developer via the Admin SDK. Nested JSON objects are represented as dictionaries, and nested JSON arrays are represented
    /// as lists.
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
    string? SignInProvider { get; }

    /// <summary>
    /// Stores sign-in second factor through which the token was obtained.
    /// </summary>
    string? SignInSecondFactor { get; }

    /// <summary>
    /// Stores the JWT string of the ID token.
    /// </summary>
    string? Token { get; }
}