using System.Net;
using System.Text;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Storage;

public sealed partial class StorageFixture
{
    [Fact]
    public async Task lists_files_with_limit()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath("files_to_keep");

        var result = await reference.ListAsync(2);
        Assert.Equal(2, result.Items.Count());
        Assert.False(string.IsNullOrWhiteSpace(result.PageToken));
    }


    [Fact]
    public async Task lists_all_files()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath("files_to_keep");

        var result = await reference.ListAllAsync();
        Assert.Equal(3, result.Items.Count());
        Assert.Empty(result.Prefixes);
        Assert.Null(result.PageToken);
    }


    [Fact]
    public async Task lists_prefixes_for_nested_files()
    {
        var root = CrossFirebaseStorage.Current.GetRootReference();
        var parent = root.GetChild("prefix_listing");
        var firstFile = parent.GetChild("folder_a/first.txt");
        var secondFile = parent.GetChild("folder_b/second.txt");

        try {
            await firstFile.PutBytes("first"u8.ToArray()).AwaitAsync();
            await secondFile.PutBytes("second"u8.ToArray()).AwaitAsync();

            var result = await parent.ListAllAsync();
            var prefixes = result.Prefixes.Select(x => x.FullPath).OrderBy(x => x).ToList();

            Assert.Empty(result.Items);
            Assert.Equal(Expected, prefixes);
        }
        finally {
            await StorageTestPathScope.DeleteIfExistsAsync(firstFile);
            await StorageTestPathScope.DeleteIfExistsAsync(secondFile);
        }
    }


    [Fact]
    public async Task deletes_file()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath("files_to_delete");

        Assert.Empty((await reference.ListAllAsync()).Items);
        await reference.GetChild("text.txt").PutBytes("This file should get deleted"u8.ToArray()).AwaitAsync();
        Assert.Single((await reference.ListAllAsync()).Items);

        await reference.GetChild("text.txt").DeleteAsync();
        Assert.Empty((await reference.ListAllAsync()).Items);
    }

}