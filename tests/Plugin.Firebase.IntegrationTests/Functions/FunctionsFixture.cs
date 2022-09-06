using Plugin.Firebase.Functions;

namespace Plugin.Firebase.IntegrationTests.Functions
{
    [Preserve(AllMembers = true)]
    public sealed class FunctionsFixture
    {
        [Fact]
        public async Task executes_simple_callable_function()
        {
            var sut = CrossFirebaseFunctions.Current;
            await sut.GetHttpsCallable("convertToLeet").CallAsync();
        }

        [Fact]
        public async Task executes_callable_function_with_json_body()
        {
            var sut = CrossFirebaseFunctions.Current;
            var json = new SimpleRequestData(123).ToJson();
            await sut.GetHttpsCallable("convertToLeet").CallAsync(json);
        }

        [Fact]
        public async Task executes_callable_function_with_json_body_and_response()
        {
            var sut = CrossFirebaseFunctions.Current;
            var json = new SimpleRequestData(123).ToJson();
            var response = await sut.GetHttpsCallable("convertToLeet").CallAsync<SimpleResponseData>(json);

            Assert.Equal(123, response.InputValue);
            Assert.Equal(1337, response.OutputValue);
        }

        [Fact]
        public async Task throws_exception_when_function_does_not_exist()
        {
            var sut = CrossFirebaseFunctions.Current;
            await Assert.ThrowsAnyAsync<Exception>(() => sut.GetHttpsCallable("doesNotExist").CallAsync());
        }
    }
}