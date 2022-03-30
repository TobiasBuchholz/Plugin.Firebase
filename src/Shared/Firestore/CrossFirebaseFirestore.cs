using System;

namespace Plugin.Firebase.Firestore
{
    public sealed class CrossFirebaseFirestore
    {
        private static Lazy<IFirebaseFirestore> _implementation = new Lazy<IFirebaseFirestore>(CreateInstance, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        private static IFirebaseFirestore CreateInstance()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0
            return null;
#else
#pragma warning disable IDE0022 // Use expression body for methods
            return new FirebaseFirestoreImplementation();
#pragma warning restore IDE0022 // Use expression body for methods
#endif
        }

        /// <summary>
        /// Gets if the plugin is supported on the current platform.
        /// </summary>
        public static bool IsSupported => _implementation.Value != null;

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static IFirebaseFirestore Current {
            get {
                var ret = _implementation.Value;
                if(ret == null) {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        private static Exception NotImplementedInReferenceAssembly() =>
            new NotImplementedException("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");

        /// <summary>
        /// Dispose of everything 
        /// </summary>
        public static void Dispose()
        {
            if(_implementation != null && _implementation.IsValueCreated) {
                _implementation.Value.Dispose();
                _implementation = new Lazy<IFirebaseFirestore>(CreateInstance, System.Threading.LazyThreadSafetyMode.PublicationOnly);
            }
        }
    }
}