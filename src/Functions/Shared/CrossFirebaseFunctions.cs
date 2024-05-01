namespace Plugin.Firebase.Functions;

public sealed class CrossFirebaseFunctions
{
    private static Lazy<IFirebaseFunctions> _implementation = new Lazy<IFirebaseFunctions>(() => CreateInstance("us-central1"), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    public static void Initialize(string region)
    {
        _implementation = new Lazy<IFirebaseFunctions>(() => CreateInstance(region), System.Threading.LazyThreadSafetyMode.PublicationOnly);
    }

    private static IFirebaseFunctions CreateInstance(string region)
    {
#if IOS || ANDROID
        return new FirebaseFunctionsImplementation(region);
#else
        return null;
#endif
    }

    public static bool IsSupported => _implementation.Value != null;

    public static IFirebaseFunctions Current {
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

    public static void Dispose()
    {
        if(_implementation != null && _implementation.IsValueCreated) {
            _implementation.Value.Dispose();
            _implementation = new Lazy<IFirebaseFunctions>(() => CreateInstance("us-central1"), System.Threading.LazyThreadSafetyMode.PublicationOnly);
        }
    }
}
