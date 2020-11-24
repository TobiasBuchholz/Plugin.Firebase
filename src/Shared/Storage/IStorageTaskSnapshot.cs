using System;

namespace Plugin.Firebase.Storage
{
    public interface IStorageTaskSnapshot
    {
        long TransferredUnitCount { get; }
        long TotalUnitCount { get; }
        double TransferredFraction { get; }
        
        Exception Error { get; }
    }
}