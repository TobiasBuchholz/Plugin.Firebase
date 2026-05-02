using Foundation;
using Plugin.Firebase.Core.Exceptions;
using NativeRemoteConfigUpdate = global::Firebase.RemoteConfig.RemoteConfigUpdate;

namespace Plugin.Firebase.RemoteConfig.Platforms.iOS;

internal static class ConfigUpdateListener
{
    internal static void OnConfigUpdate(
        NativeRemoteConfigUpdate? configUpdate,
        NSError? error,
        Action<RemoteConfigUpdate> onUpdate,
        Action<Exception>? onError
    )
    {
        if(error != null) {
            onError?.Invoke(ToAbstract(error));
        } else if(configUpdate != null) {
            onUpdate(ToAbstract(configUpdate.UpdatedKeys));
        } else {
            onError?.Invoke(new FirebaseException("Remote Config update is null"));
        }
    }

    internal static RemoteConfigUpdate ToAbstract(NSSet<NSString> updatedKeys)
    {
        return new RemoteConfigUpdate(updatedKeys.ToArray().Select(x => (string) x).ToArray());
    }

    internal static FirebaseException ToAbstract(NSError error)
    {
        return new FirebaseException(error.LocalizedDescription ?? "Unknown Remote Config update error");
    }
}
