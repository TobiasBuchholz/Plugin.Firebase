namespace Plugin.Firebase.RemoteConfig;

/// <summary>
/// Represents a real-time Remote Config update.
/// </summary>
public sealed class RemoteConfigUpdate
{
    /// <summary>
    /// Creates a new <c>RemoteConfigUpdate</c> instance.
    /// </summary>
    /// <param name="updatedKeys">Remote Config parameter keys that changed in the update.</param>
    public RemoteConfigUpdate(IEnumerable<string> updatedKeys)
    {
        UpdatedKeys = updatedKeys;
    }

    /// <summary>
    /// Gets the Remote Config parameter keys that changed in the update.
    /// </summary>
    public IEnumerable<string> UpdatedKeys { get; }
}