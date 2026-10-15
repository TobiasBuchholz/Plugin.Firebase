using System.Globalization;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.UnitTests;

public class StorageExceptionFactoryTests
{
    [Fact]
    public void create_passes_native_metadata_through()
    {
        var inner = new InvalidOperationException("inner");

        var exception = StorageExceptionFactory.Create(
            "NSError",
            "Object missing.txt does not exist.",
            inner,
            "FIRStorageErrorDomain",
            -13010,
            404
        );

        Assert.IsType<CrossPlatformFirebaseStorageException>(exception);
        Assert.Equal("NSError", exception.NativeExceptionTypeName);
        Assert.Equal("Object missing.txt does not exist.", exception.NativeErrorMessage);
        Assert.Equal("Object missing.txt does not exist.", exception.Message);
        Assert.Equal("FIRStorageErrorDomain", exception.NativeErrorDomain);
        Assert.Equal("-13010", exception.NativeErrorCode);
        Assert.Equal(404, exception.NativeHttpResultCode);
        Assert.Same(inner, exception.InnerException);
    }

    [Fact]
    public void create_without_code_leaves_codes_and_domain_null()
    {
        var exception = StorageExceptionFactory.Create(
            "com.google.firebase.FirebaseException",
            nativeErrorMessage: null,
            new InvalidOperationException("inner"),
            nativeErrorDomain: null,
            nativeErrorCode: null,
            nativeHttpResultCode: null
        );

        Assert.Null(exception.NativeErrorDomain);
        Assert.Null(exception.NativeErrorCode);
        Assert.Null(exception.NativeHttpResultCode);
        Assert.Equal("inner", exception.NativeErrorMessage);
    }

    [Theory]
    [InlineData("sv-SE")]
    [InlineData("nb-NO")]
    [InlineData("fi-FI")]
    [InlineData("fa-IR")]
    [InlineData("ar-SA")]
    public void create_formats_negative_error_code_with_invariant_culture(string cultureName)
    {
        var culture = GetCultureOrSkip(cultureName);
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = culture;
        try {
            var exception = StorageExceptionFactory.Create(
                "com.google.firebase.storage.StorageException",
                "Object does not exist at location.",
                new InvalidOperationException("inner"),
                nativeErrorDomain: null,
                nativeErrorCode: -13010,
                nativeHttpResultCode: 404
            );

            Assert.Equal("-13010", exception.NativeErrorCode);
        }
        finally {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void sv_se_formats_negative_numbers_with_a_different_minus_sign()
    {
        // Shows that the culture test above can fail on this host. Without ICU culture data every culture formats
        // "-13010", so that test would pass even without the invariant-culture formatting.
        var culture = GetCultureOrSkip("sv-SE");
        Assert.SkipUnless(
            culture.NumberFormat.NegativeSign == "\u2212",
            "The host's sv-SE culture data doesn't use U+2212 as the negative sign, for example because ICU isn't available."
        );

        Assert.NotEqual("-13010", (-13010L).ToString(culture));
    }

    private static CultureInfo GetCultureOrSkip(string cultureName)
    {
        try {
            return new CultureInfo(cultureName);
        } catch(CultureNotFoundException) {
            Assert.Skip(
                $"Culture {cultureName} isn't available on this host, for example because DOTNET_SYSTEM_GLOBALIZATION_INVARIANT is set."
            );
            throw;
        }
    }
}