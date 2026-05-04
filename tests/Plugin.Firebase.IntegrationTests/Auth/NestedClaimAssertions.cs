namespace Plugin.Firebase.IntegrationTests.Auth
{
    internal static class NestedClaimAssertions
    {
        public static void AssertNestedCustomClaim(IDictionary<string, object> nestedObject)
        {
            Assert.True(Convert.ToBoolean(nestedObject["enabled"]));

            var roles = Assert.IsAssignableFrom<IList<object>>(nestedObject["roles"]);
            Assert.Equal(["admin", "tester"], roles.Select(x => Assert.IsType<string>(x)));

            var metadata = Assert.IsAssignableFrom<IDictionary<string, object>>(
                nestedObject["metadata"]
            );
            Assert.Equal("emulator", Assert.IsType<string>(metadata["source"]));
            Assert.Equal(2L, Convert.ToInt64(metadata["version"]));

            var history = Assert.IsAssignableFrom<IList<object>>(nestedObject["history"]);
            Assert.Collection(
                history,
                item => AssertHistoryItem(item, "created", 1),
                item => AssertHistoryItem(item, "updated", 2)
            );

            Assert.Equal(7L, Convert.ToInt64(nestedObject["score"]));
            Assert.Equal(1.5, Convert.ToDouble(nestedObject["ratio"]), precision: 3);
            Assert.Null(nestedObject["optional"]);
        }

        public static void AssertNestedCustomArray(IList<object> nestedArray)
        {
            Assert.Collection(
                nestedArray,
                item => {
                    var dict = Assert.IsAssignableFrom<IDictionary<string, object>>(item);
                    Assert.Equal("first", Assert.IsType<string>(dict["name"]));

                    var flags = Assert.IsAssignableFrom<IList<object>>(dict["flags"]);
                    Assert.Equal([true, false], flags.Select(Convert.ToBoolean));
                },
                item => {
                    var dict = Assert.IsAssignableFrom<IDictionary<string, object>>(item);
                    Assert.Equal("second", Assert.IsType<string>(dict["name"]));

                    var metadata = Assert.IsAssignableFrom<IDictionary<string, object>>(
                        dict["metadata"]
                    );
                    Assert.Equal("emulator", Assert.IsType<string>(metadata["source"]));
                }
            );
        }

        private static void AssertHistoryItem(object item, string action, int count)
        {
            var dict = Assert.IsAssignableFrom<IDictionary<string, object>>(item);
            Assert.Equal(action, Assert.IsType<string>(dict["action"]));
            Assert.Equal(count, Convert.ToInt32(dict["count"]));
        }
    }
}