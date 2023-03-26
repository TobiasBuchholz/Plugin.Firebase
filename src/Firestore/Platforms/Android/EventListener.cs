using Firebase.Firestore;
using Object = Java.Lang.Object;
using IEventListener = Firebase.Firestore.IEventListener;

namespace Plugin.Firebase.Firestore.Platforms.Android;

public sealed class EventListener : Object, IEventListener
{
    private readonly Action<Object> _action;
    private readonly Action<FirebaseFirestoreException> _errorAction;

    public EventListener(Action<Object> action, Action<FirebaseFirestoreException> errorAction)
    {
        _action = action;
        _errorAction = errorAction;
    }

    public void OnEvent(Object value, FirebaseFirestoreException error)
    {
        if(value != null) {
            _action(value);
        }

        if(error != null) {
            _errorAction(error);
        }
    }
}