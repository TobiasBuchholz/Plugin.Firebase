using System.Net;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Storage;

internal static class StorageAssertions
{
    public static string ExpectedBucket()
    {
        return CrossFirebaseStorage.Current.GetRootReference().Bucket;
    }

    public static void DownloadUrl(string pathToFile, string downloadUrl)
    {
        var bucket = ExpectedBucket();
        var decodedUrl = WebUtility.UrlDecode(downloadUrl);
        if(IntegrationTestEnvironment.ShouldUseStorageEmulator) {
            var uri = new Uri(decodedUrl);
            var expectedEndpoint = IntegrationTestEnvironment.StorageEmulatorEndpoint;

            Assert.Equal("http", uri.Scheme);
            Assert.True(
                string.Equals(uri.Host, expectedEndpoint.Host, StringComparison.OrdinalIgnoreCase)
                || (string.Equals(expectedEndpoint.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)),
                $"Expected storage emulator host '{expectedEndpoint.Host}' but got '{uri.Host}'.");
            Assert.Equal(expectedEndpoint.Port, uri.Port);
            Assert.StartsWith($"/v0/b/{bucket}/o/{pathToFile}", uri.AbsolutePath);
            Assert.Contains("alt=media", uri.Query, StringComparison.Ordinal);
            Assert.Contains("token=", uri.Query, StringComparison.Ordinal);
            return;
        }

        var port = DeviceInfo.Platform == DevicePlatform.iOS ? ":443" : "";
        Assert.StartsWith(
            $"https://firebasestorage.googleapis.com{port}/v0/b/{bucket}/o/{pathToFile}?alt=media&token=",
            decodedUrl);
    }

    // Pins the failure each platform reports today for a PutFile source that doesn't exist. Once #741 lands,
    // both platforms throw CrossPlatformFirebaseStorageException wrapping these native errors instead.
    public static void MissingUploadFileFailure(Exception exception, string filePath)
    {
#if ANDROID
        var storageException = Assert.IsType<global::Firebase.Storage.StorageException>(exception);
        Assert.Equal(global::Firebase.Storage.StorageException.ErrorUnknown, storageException.ErrorCode);
        Assert.IsType<Java.IO.FileNotFoundException>(storageException.Cause);
#elif IOS
        // The iOS transfer task rethrows the native NSError's localized description.
        Assert.IsType<Exception>(exception);
        Assert.Contains("is not reachable", exception.Message, StringComparison.Ordinal);
        Assert.Contains(Path.GetFileName(filePath), exception.Message, StringComparison.Ordinal);
#else
        throw new PlatformNotSupportedException("Native Storage upload failures are only defined on Android and iOS.");
#endif
    }
}