using Plugin.Firebase.DynamicLinks.EventArgs;

namespace Plugin.Firebase.DynamicLinks;

/// <summary>
/// Provides access to dynamic links that are received by an app at launch.
/// </summary>
public interface IFirebaseDynamicLinks : IDisposable
{
    /// <summary>
    /// Returns the current Dynamic Link.
    /// </summary>
    string GetDynamicLink();

    /// <summary>
    /// Returns an <c>IDynamicLinkBuilder</c> object to create a Dynamic Link.
    /// </summary>
    /// <returns></returns>
    IDynamicLinkBuilder CreateDynamicLink();

    /// <summary>
    /// Gets invoked when the app was triggered by a Dynamic Link.
    /// </summary>
    event EventHandler<DynamicLinkReceivedEventArgs> DynamicLinkReceived;

    /// <summary>
    /// Gets invoked when something went wrong during handling a Dynamic Link.
    /// </summary>
    event EventHandler<DynamicLinkFailedEventArgs> DynamicLinkFailed;
}