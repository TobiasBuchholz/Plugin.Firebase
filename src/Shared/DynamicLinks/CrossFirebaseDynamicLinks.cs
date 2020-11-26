using System;

namespace Plugin.Firebase.DynamicLinks
{
    public sealed class CrossFirebaseDynamicLinks
    {
        static readonly Lazy<IFirebaseDynamicLinks> _implementation = new Lazy<IFirebaseDynamicLinks>(CreateInstance, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Gets if the plugin is supported on the current platform.
        /// </summary>
        public static bool IsSupported => _implementation.Value != null;

        /// <summary>
        /// Current plugin implementation to use
        /// </summary>
        public static IFirebaseDynamicLinks Current {
            get {
                var ret = _implementation.Value;
                if (ret == null) {
                    throw new NotImplementedException();
                }
                return ret;
            }
        }

        private static IFirebaseDynamicLinks CreateInstance()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0
            return null;
#else
#pragma warning disable IDE0022 // Use expression body for methods
			return new FirebaseDynamicLinksImplementation();
#pragma warning restore IDE0022 // Use expression body for methods
#endif
        } 
    }
}