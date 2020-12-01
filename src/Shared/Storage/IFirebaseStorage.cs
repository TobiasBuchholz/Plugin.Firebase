using System;

namespace Plugin.Firebase.Storage
{
    public interface IFirebaseStorage : IDisposable
    {
        IStorageReference GetRootReference();
        IStorageReference GetReferenceFromUrl(string url);
        IStorageReference GetReferenceFromPath(string path);
    }
}