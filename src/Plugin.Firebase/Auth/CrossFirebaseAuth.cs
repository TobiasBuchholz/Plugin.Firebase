using System;
using Plugin.Firebase.Abstractions.Auth;

namespace Plugin.Firebase.Auth
{
    public sealed class CrossFirebaseAuth
    {
        static Lazy<IFirebaseAuth> implementation = new Lazy<IFirebaseAuth>(() => CreateFirebaseAuth(), System.Threading.LazyThreadSafetyMode.PublicationOnly);


        /// <summary>
        /// Gets if the plugin is supported on the current platform.
        /// </summary>
        public static bool IsSupported => implementation.Value == null ? false : true;

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static IFirebaseAuth Current
        {
            get
            {
                var ret = implementation.Value;
                if (ret == null) {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        static IFirebaseAuth CreateFirebaseAuth()
        {
#if NETSTANDARD2_0
            return null;
#else
#pragma warning disable IDE0022 // Use expression body for methods
            return new FirebaseAuthImplementation();
#pragma warning restore IDE0022 // Use expression body for methods
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly() =>
            new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");


        /// <summary>
        /// Dispose of everything 
        /// </summary>
        public static void Dispose()
        {
            if(implementation != null && implementation.IsValueCreated) {
                implementation.Value.Dispose();

                implementation = new Lazy<IFirebaseAuth>(() => CreateFirebaseAuth(), System.Threading.LazyThreadSafetyMode.PublicationOnly);
            }
        }
    }
}
