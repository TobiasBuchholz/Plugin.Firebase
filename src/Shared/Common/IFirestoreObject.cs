namespace Plugin.Firebase.Common
{
    public interface IFirestoreObject<out T> : IFirestoreObject
    {
        string Id { get; }
        
        T WithId(string id);
    }

    public interface IFirestoreObject
    {
    }
}