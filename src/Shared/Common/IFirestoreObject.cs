namespace Plugin.Firebase.Common
{
    public interface IFirestoreObject<out T>
    {
        string Id { get; }

        T WithId(string id);
    }
}