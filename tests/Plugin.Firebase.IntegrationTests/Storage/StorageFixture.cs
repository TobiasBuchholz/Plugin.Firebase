using System.Net;
using System.Text;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Storage
{
    [Collection("Sequential")]
    [TestLogging]
    [Preserve(AllMembers = true)]
    public sealed class StorageFixture : IAsyncLifetime
    {
        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public void gets_root_reference()
        {
            var reference = CrossFirebaseStorage.Current.GetRootReference();

            Assert.NotNull(reference);
            Assert.Null(reference.Parent);
            Assert.Equal("/", reference.FullPath);
            Assert.Equal("", reference.Name);
            Assert.Equal(GetExpectedBucket(), reference.Bucket);
        }

        [Fact]
        public void gets_reference_from_url()
        {
            var bucket = GetExpectedBucket();
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromUrl($"gs://{bucket}/files_to_keep/text_1.txt");

            Assert.NotNull(reference.Root);
            Assert.NotNull(reference.Parent);
            Assert.Equal("/files_to_keep/text_1.txt", reference.FullPath);
            Assert.Equal("text_1.txt", reference.Name);
            Assert.Equal(bucket, reference.Bucket);
        }

        [Fact]
        public void gets_reference_from_path()
        {
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath("files_to_keep/text_1.txt");

            Assert.NotNull(reference.Root);
            Assert.NotNull(reference.Parent);
            Assert.Equal("/files_to_keep/text_1.txt", reference.FullPath);
            Assert.Equal("text_1.txt", reference.Name);
            Assert.Equal(GetExpectedBucket(), reference.Bucket);
        }

        [Fact]
        public void gets_child_reference()
        {
            var reference = CrossFirebaseStorage
                .Current
                .GetRootReference().GetChild("files_to_keep/text_1.txt");

            Assert.NotNull(reference.Root);
            Assert.NotNull(reference.Parent);
            Assert.Equal("/files_to_keep/text_1.txt", reference.FullPath);
            Assert.Equal("text_1.txt", reference.Name);
            Assert.Equal(GetExpectedBucket(), reference.Bucket);
        }

        [Fact]
        public async Task gets_download_url()
        {
            var path = $"files_to_keep/text_1.txt";
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath(path);

            var downloadUrl = await reference.GetDownloadUrlAsync();
            AssertDownloadUrl(path, downloadUrl);
        }

        private static void AssertDownloadUrl(string pathToFile, string downloadUrl)
        {
            var bucket = GetExpectedBucket();
            var decodedUrl = WebUtility.UrlDecode(downloadUrl);
            if(UsesStorageEmulator()) {
                var uri = new Uri(decodedUrl);
                var expectedHost = GetStorageEmulatorHost();
                var expectedPort = GetStorageEmulatorPort();

                Assert.Equal("http", uri.Scheme);
                Assert.True(
                    string.Equals(uri.Host, expectedHost, StringComparison.OrdinalIgnoreCase)
                        || (string.Equals(expectedHost, "localhost", StringComparison.OrdinalIgnoreCase)
                            && string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)),
                    $"Expected storage emulator host '{expectedHost}' but got '{uri.Host}'.");
                Assert.Equal(expectedPort, uri.Port);
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

        private static string GetExpectedBucket()
        {
            return CrossFirebaseStorage.Current.GetRootReference().Bucket;
        }

        private static bool UsesStorageEmulator()
        {
            return Environment.GetEnvironmentVariable("PLUGIN_FIREBASE_USE_STORAGE_EMULATOR") == "1";
        }

        private static string GetStorageEmulatorHost()
        {
            var host = Environment.GetEnvironmentVariable("PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST");
            return string.IsNullOrWhiteSpace(host)
                ? OperatingSystem.IsAndroid() ? "10.0.2.2" : "localhost"
                : host;
        }

        private static int GetStorageEmulatorPort()
        {
            var portValue = Environment.GetEnvironmentVariable("PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT");
            return string.IsNullOrWhiteSpace(portValue) ? 9199 : int.Parse(portValue);
        }

        [Fact]
        public async Task uploads_via_byte_array()
        {
            var path = $"texts/via_bytes.txt";
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath(path);

            await reference.PutBytes(Encoding.UTF8.GetBytes("Some test text")).AwaitAsync();
            var downloadUrl = await reference.GetDownloadUrlAsync();
            AssertDownloadUrl(path, downloadUrl);
        }

        [Fact]
        public async Task uploads_via_stream()
        {
            var path = $"texts/via_stream.txt";
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath(path);

            using(var stream = await CreateTextStreamAsync("Some text via stream")) {
                await reference.PutStream(stream).AwaitAsync();
                var downloadUrl = await reference.GetDownloadUrlAsync();
                AssertDownloadUrl(path, downloadUrl);
            }
        }

        private static async Task<Stream> CreateTextStreamAsync(string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(text);
            await writer.FlushAsync();
            return stream;
        }

        [Fact]
        public async Task uploads_stream_with_meta_data()
        {
            var path = $"texts/via_stream_with_metadata.txt";
            var metadata = new StorageMetadata(contentType: "text/plain");
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath(path);

            await reference.PutBytes("Some test text"u8.ToArray(), metadata).AwaitAsync();
            var uploadedMetadata = await reference.GetMetadataAsync();

            Assert.Equal(path, uploadedMetadata.Path);
            Assert.Equal("text/plain", uploadedMetadata.ContentType);
            Assert.Equal(14, uploadedMetadata.Size);

            var customData = new Dictionary<string, string> { { "some_key", "some_value" } };
            var updatedMetadata = await reference.UpdateMetadataAsync(new StorageMetadata(contentType: "text/html", customMetadata: customData));

            Assert.Equal(path, updatedMetadata.Path);
            Assert.Equal("text/html", updatedMetadata.ContentType);
            Assert.Equal(customData, updatedMetadata.CustomMetadata);
        }

        [Fact]
        public async Task lists_files_with_limit()
        {
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath("files_to_keep");

            var result = await reference.ListAsync(2);
            Assert.Equal(2, result.Items.Count());
        }

        [Fact]
        public async Task lists_all_files()
        {
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath("files_to_keep");

            var result = await reference.ListAllAsync();
            Assert.Equal(3, result.Items.Count());
        }

        [Fact]
        public async Task gets_data_as_stream()
        {
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath("files_to_keep/text_1.txt");

            var stream = await reference.GetStreamAsync(1 * 1024 * 1024);
            Assert.NotNull(stream);
        }

        [Fact]
        public async Task gets_data_as_bytes()
        {
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath("files_to_keep/text_1.txt");

            var bytes = await reference.GetBytesAsync(1 * 1024 * 1024);
            Assert.NotNull(bytes);
            Assert.Equal(34, bytes.Length);
        }

        [Fact]
        public async Task downloads_file()
        {
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath("files_to_keep/text_1.txt");

            await reference.DownloadFile($"{FileSystem.CacheDirectory}/test.txt").AwaitAsync();
        }

        [Fact]
        public void can_manage_files_upload()
        {
            var path = $"texts/managed.txt";
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath(path);

            var transferTask = reference.PutBytes(Encoding.UTF8.GetBytes("Some test text"));
            transferTask.Pause();
            transferTask.Resume();
            transferTask.Cancel();
        }

        [Fact]
        public void can_manage_files_download()
        {
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath("files_to_keep/text_1.txt");

            var transferTask = reference.DownloadFile($"{FileSystem.CacheDirectory}/managed.txt");
            transferTask.Pause();
            transferTask.Resume();
            transferTask.Cancel();
        }

        [Fact]
        public async Task deletes_file()
        {
            var reference = CrossFirebaseStorage
                .Current
                .GetReferenceFromPath("files_to_delete");

            Assert.Empty((await reference.ListAllAsync()).Items);
            await reference.GetChild("text.txt").PutBytes(Encoding.UTF8.GetBytes("This file should get deleted")).AwaitAsync();
            Assert.Single((await reference.ListAllAsync()).Items);

            await reference.GetChild("text.txt").DeleteAsync();
            Assert.Empty((await reference.ListAllAsync()).Items);
        }

        public async Task DisposeAsync()
        {
            TestLog.Write("[STORAGE CLEANUP START]");
            var rootReference = CrossFirebaseStorage.Current.GetRootReference();
            var filesToDelete = await ListItemsIfExistsAsync(rootReference.GetChild("files_to_delete"));
            var texts = await ListItemsIfExistsAsync(rootReference.GetChild("texts"));
            await Task.WhenAll(filesToDelete.Select(TryDeleteAsync).Concat(texts.Select(TryDeleteAsync)));
            TestLog.Write("[STORAGE CLEANUP END]");
        }

        private static async Task<IEnumerable<IStorageReference>> ListItemsIfExistsAsync(IStorageReference reference)
        {
            try {
                return (await reference.ListAllAsync()).Items;
            } catch(Exception e) when (e.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)) {
                TestLog.Write($"[STORAGE CLEANUP SKIP] {reference.FullPath}: {e.Message}");
                return Array.Empty<IStorageReference>();
            }
        }

        private static async Task TryDeleteAsync(IStorageReference reference)
        {
            try {
                await reference.DeleteAsync();
            } catch(Exception e) {
                TestLog.Write($"[STORAGE CLEANUP ERROR] {reference.FullPath}: {e}");
            }
        }
    }
}
