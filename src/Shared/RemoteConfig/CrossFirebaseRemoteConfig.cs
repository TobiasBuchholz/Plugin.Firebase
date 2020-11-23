using System;
using System.Collections.Generic;
using System.Linq;
using Plugin.Firebase.RemoteConfig;

namespace Plugin.Firebase.Shared.RemoteConfig
{
    public sealed class CrossFirebaseRemoteConfig
    {
        static readonly Lazy<IFirebaseRemoteConfig> _implementation = new Lazy<IFirebaseRemoteConfig>(CreateInstance, System.Threading.LazyThreadSafetyMode.PublicationOnly);


        /// <summary>
        /// Gets if the plugin is supported on the current platform.
        /// </summary>
        public static bool IsSupported => _implementation.Value != null;

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static IFirebaseRemoteConfig Current
        {
            get
            {
                //new Dictionary<string, string>().ToList();
                var ret = _implementation.Value;
                if (ret == null) {
                    throw new NotImplementedException();
                }
                return ret;
            }
        }

        private static IFirebaseRemoteConfig CreateInstance()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0
            return null;
#else
#pragma warning disable IDE0022 // Use expression body for methods
			return new FirebaseRemoteConfigImplementation();
#pragma warning restore IDE0022 // Use expression body for methods
#endif
        } 
    }
}