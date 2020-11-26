using System;

namespace Plugin.Firebase.Storage
{
    public interface IFirebaseStorage : IDisposable
    {
        IStorageReference GetRootReference();
    }
}