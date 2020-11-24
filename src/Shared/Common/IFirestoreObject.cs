namespace Plugin.Firebase.Abstractions.Common
{
    public interface IFirestoreObject<out T>
    {
        string Id { get; }

        T WithId(string id);
    }
}