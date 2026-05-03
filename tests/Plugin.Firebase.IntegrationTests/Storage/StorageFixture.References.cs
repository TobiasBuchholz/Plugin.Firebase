using System.Net;
using System.Text;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Storage;

public sealed partial class StorageFixture
{
    [Fact]
    public void gets_root_reference()
    {
        var reference = CrossFirebaseStorage.Current.GetRootReference();

        Assert.NotNull(reference);
        Assert.Null(reference.Parent);
        Assert.Equal("/", reference.FullPath);
        Assert.Equal("", reference.Name);
        Assert.Equal(StorageAssertions.ExpectedBucket(), reference.Bucket);
    }


    [Fact]
    public void gets_reference_from_url()
    {
        var bucket = StorageAssertions.ExpectedBucket();
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
        Assert.Equal(StorageAssertions.ExpectedBucket(), reference.Bucket);
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
        Assert.Equal(StorageAssertions.ExpectedBucket(), reference.Bucket);
    }


    [Fact]
    public void normalizes_nested_child_reference_paths()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetRootReference()
            .GetChild("/nested//folder///file.txt/");

        Assert.Equal("/nested/folder/file.txt", reference.FullPath);
        Assert.Equal("file.txt", reference.Name);
        var parent = reference.Parent;
        Assert.NotNull(parent);
        Assert.Equal("/nested/folder", parent.FullPath);
    }


    [Fact]
    public async Task gets_download_url()
    {
        const string path = "files_to_keep/text_1.txt";
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath(path);

        var downloadUrl = await reference.GetDownloadUrlAsync();
        StorageAssertions.DownloadUrl(path, downloadUrl);
    }

}