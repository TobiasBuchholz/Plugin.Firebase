namespace Plugin.Firebase.RemoteConfig;

/// <summary>
/// Firebase Remote Config settings.
/// </summary>
public sealed class RemoteConfigSettings
{
    /// <summary>
    /// Creates a new <c>RemoteConfigSettings</c> instance.
    /// </summary>
    /// <param name="minimumFetchInterval">The minimum interval between successive fetches. Defaults to 3600 seconds (1 hour).</param>
    /// <param name="fetchTimeout">The timeout for fetch operations. Defaults to 60 seconds.</param>
    public RemoteConfigSettings(
        TimeSpan? minimumFetchInterval = null,
        TimeSpan? fetchTimeout = null
    )
    {
        MinimumFetchInterval = minimumFetchInterval ?? TimeSpan.FromSeconds(3600);
        FetchTimeout = fetchTimeout ?? TimeSpan.FromSeconds(60);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if(obj is RemoteConfigSettings other) {
            return (MinimumFetchInterval, FetchTimeout).Equals(
                (other.MinimumFetchInterval, other.FetchTimeout)
            );
        }
        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return (MinimumFetchInterval, FetchTimeout).GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[{nameof(RemoteConfigSettings)}: {nameof(MinimumFetchInterval)}={MinimumFetchInterval}, {nameof(FetchTimeout)}={FetchTimeout}]";
    }

    /// <summary>
    /// Returns the minimum interval between successive fetches calls in seconds.
    /// </summary>
    public TimeSpan MinimumFetchInterval { get; }

    /// <summary>
    /// Returns the fetch timeout in seconds.
    /// </summary>
    public TimeSpan FetchTimeout { get; }
}