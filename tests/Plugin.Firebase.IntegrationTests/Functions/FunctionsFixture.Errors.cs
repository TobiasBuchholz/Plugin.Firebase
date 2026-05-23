using System.Text.Json;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Functions;

namespace Plugin.Firebase.IntegrationTests.Functions;

public sealed partial class FunctionsFixture
{
    [Fact]
    public async Task throws_exception_when_function_does_not_exist()
    {
        var sut = CrossFirebaseFunctions.Current;
        await Assert.ThrowsAnyAsync<Exception>(() => sut.GetHttpsCallable("doesNotExist").CallAsync());
    }


    [Fact]
    public async Task propagates_structured_callable_error()
    {
        var sut = CrossFirebaseFunctions.Current;

        var exception = await Assert.ThrowsAnyAsync<Exception>(
            () => sut.GetHttpsCallable("throwStructuredError").CallAsync());

        Assert.Contains(
            "acceptance",
            exception.ToString(),
            StringComparison.OrdinalIgnoreCase);
    }

}