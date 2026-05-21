using Firebase.RemoteConfig;
using Plugin.Firebase.Core.Exceptions;
using Object = Java.Lang.Object;

namespace Plugin.Firebase.RemoteConfig.Platforms.Android;

internal sealed class ConfigUpdateListener : Object, IConfigUpdateListener
{
    private readonly Action<RemoteConfigUpdate> _onUpdate;
    private readonly Action<Exception>? _onError;

    public ConfigUpdateListener(
        Action<RemoteConfigUpdate> onUpdate,
        Action<Exception>? onError
    )
    {
        _onUpdate = onUpdate;
        _onError = onError;
    }

    public void OnUpdate(ConfigUpdate configUpdate)
    {
        _onUpdate(ToAbstract(configUpdate));
    }

    public void OnError(FirebaseRemoteConfigException? error)
    {
        _onError?.Invoke(ToAbstract(error));
    }

    internal static RemoteConfigUpdate ToAbstract(ConfigUpdate configUpdate)
    {
        return new RemoteConfigUpdate(configUpdate.UpdatedKeys.ToArray());
    }

    internal static FirebaseException ToAbstract(FirebaseRemoteConfigException? error)
    {
        return new FirebaseException(error?.LocalizedMessage ?? "Unknown Remote Config update error");
    }
}
