using System.Text;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Storage;

public sealed partial class StorageFixture
{
    private const UnixFileMode ReadOnlyFileMode = UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead;

    [Fact]
    public async Task uploads_via_file_path()
    {
        const string path = "texts/via_file.txt";
        const string contents = "Some text via file";
        var filePath = await CreateTempTextFileAsync($"via_file_{Guid.NewGuid():N}.txt", contents);
        var reference = CrossFirebaseStorage.Current.GetReferenceFromPath(path);

        try {
            // iOS reads the file after PutFile returns, so it must stay in place until the upload completes.
            await reference.PutFile(filePath).AwaitAsync();
        }
        finally {
            File.Delete(filePath);
        }

        var bytes = await reference.GetBytesAsync(1 * 1024 * 1024);
        Assert.Equal(contents, Encoding.UTF8.GetString(bytes));
    }


    [Fact]
    public async Task uploads_read_only_file()
    {
        const string contents = "Some text via read-only file";
        await using var scope = StorageTestPathScope.FromPath("texts");
        var reference = scope.Child("via_read_only_file.txt");
        var filePath = await CreateTempTextFileAsync($"read_only_{Guid.NewGuid():N}.txt", contents);
        File.SetUnixFileMode(filePath, ReadOnlyFileMode);

        try {
            await reference.PutFile(filePath).AwaitAsync();
        }
        finally {
            File.Delete(filePath);
        }

        var bytes = await reference.GetBytesAsync(1 * 1024 * 1024);
        Assert.Equal(contents, Encoding.UTF8.GetString(bytes));
    }


    [Fact]
    public async Task file_upload_without_content_type_uses_platform_default()
    {
        await using var scope = StorageTestPathScope.FromPath("texts");
        var reference = scope.Child("file_default_content_type.txt");
        var filePath = await CreateTempTextFileAsync($"default_content_type_{Guid.NewGuid():N}.txt", "plain text");

        try {
            await reference.PutFile(filePath).AwaitAsync();
        }
        finally {
            File.Delete(filePath);
        }

        // Native iOS infers the content type from the file extension; native Android stores the default type.
        var expectedContentType = OperatingSystem.IsIOS() ? "text/plain" : "application/octet-stream";
        var metadata = await reference.GetMetadataAsync();
        Assert.Equal(expectedContentType, metadata.ContentType);
    }


    [Fact]
    public async Task file_upload_keeps_explicit_content_type()
    {
        await using var scope = StorageTestPathScope.FromPath("texts");
        var reference = scope.Child("file_explicit_content_type.txt");
        var filePath = await CreateTempTextFileAsync($"explicit_content_type_{Guid.NewGuid():N}.txt", "{}");

        try {
            await reference.PutFile(filePath, new StorageMetadata(contentType: "application/json")).AwaitAsync();
        }
        finally {
            File.Delete(filePath);
        }

        var metadata = await reference.GetMetadataAsync();
        Assert.Equal("application/json", metadata.ContentType);
    }


    [Fact]
    public async Task reports_missing_upload_file_when_awaited()
    {
        await using var scope = StorageTestPathScope.FromPath("texts");
        var reference = scope.Child("missing_upload_file.txt");
        var filePath = Path.Combine(FileSystem.CacheDirectory, $"missing_{Guid.NewGuid():N}.txt");

        var transferTask = reference.PutFile(filePath);

        await Assert.ThrowsAnyAsync<Exception>(() => transferTask.AwaitAsync());
    }


    [IosFact]
    public void rejects_empty_upload_file_path_on_ios()
    {
        var reference = CrossFirebaseStorage.Current.GetReferenceFromPath("texts/empty_upload_file_path.txt");

        Assert.Throws<ArgumentException>(() => reference.PutFile(""));
    }
}