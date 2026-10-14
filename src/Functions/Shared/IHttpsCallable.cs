namespace Plugin.Firebase.Functions;

/// <summary>
/// A reference to a particular Callable HTTPS trigger in Cloud Functions.
/// </summary>
public interface IHttpsCallable
{
    /// <summary>
    /// Executes this Callable HTTPS trigger asynchronously without any parameters. The request to the Cloud Functions
    /// backend made by this method automatically includes a Firebase Instance ID token to identify the app instance.
    /// If a user is logged in with Firebase Auth, an auth ID token for the user is also automatically included.
    /// </summary>
    /// <param name="dataJson">
    /// Optional request data in JSON format. It can be any JSON value (object, array, string, number, boolean or null).
    /// Malformed JSON, such as an unquoted string, throws before the request is sent. The exception type differs by
    /// platform: Android surfaces Gson's <c>JsonSyntaxException</c>, iOS throws a <c>FirebaseException</c>.
    /// </param>
    Task CallAsync(string dataJson = null);

    /// <summary>
    /// Executes this Callable HTTPS trigger asynchronously without any parameters. The request to the Cloud Functions
    /// backend made by this method automatically includes a Firebase Instance ID token to identify the app instance.
    /// If a user is logged in with Firebase Auth, an auth ID token for the user is also automatically included.
    /// </summary>
    /// <param name="dataJson">
    /// Optional request data in JSON format. It can be any JSON value (object, array, string, number, boolean or null).
    /// Malformed JSON, such as an unquoted string, throws before the request is sent. The exception type differs by
    /// platform: Android surfaces Gson's <c>JsonSyntaxException</c>, iOS throws a <c>FirebaseException</c>.
    /// </param>
    /// <typeparam name="TResponse">The type that gets returned in a successful call by the Cloud Function.</typeparam>
    Task<TResponse> CallAsync<TResponse>(string dataJson = null);
}