using System;

namespace Plugin.Firebase.Storage
{
    public sealed class CrossFirebaseStorage
    {
        static readonly Lazy<IFirebaseStorage> _implementation = new Lazy<IFirebaseStorage>(CreateInstance, System.Threading.LazyThreadSafetyMode.PublicationOnly);


        /// <summary>
        /// Gets if the plugin is supported on the current platform.
        /// </summary>
        public static bool IsSupported => _implementation.Value != null;

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static IFirebaseStorage Current {
            get {
                var ret = _implementation.Value;
                if(ret == null) {
                    throw new NotImplementedException();
                }
                return ret;
            }
        }

        private static IFirebaseStorage CreateInstance()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0
            return null;
#else
#pragma warning disable IDE0022 // Use expression body for methods
            return new FirebaseStorageImplementation();
#pragma warning restore IDE0022 // Use expression body for methods
#endif
        }
    }
}