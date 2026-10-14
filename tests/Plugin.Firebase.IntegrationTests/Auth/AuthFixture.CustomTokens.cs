using System.Text.Json;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Functions;
using Plugin.Firebase.IntegrationTests.Functions;

namespace Plugin.Firebase.IntegrationTests.Auth
{
    public sealed partial class AuthFixture
    {
        [EmulatorBackendFact]
        public async Task signs_in_user_via_custom_token()
        {
            var auth = CrossFirebaseAuth.Current;
            var uid = IntegrationTestData.UniqueId("custom-token");
            var claims = AuthTestPayloads.CreateNestedCustomClaims();
            var requestJson = JsonSerializer.Serialize(new {
                uid,
                claims,
            });
            var response = await CrossFirebaseFunctions.Current
                .GetHttpsCallable("createCustomToken")
                .CallAsync<CustomTokenResponseData>(requestJson);

            await auth.SignOutAsync();
            var user = await auth.SignInWithCustomTokenAsync(response.Token);
            await using var testUser = AuthTestUserScope.TrackCurrentUser(auth);
            var idTokenResult = await user.GetIdTokenResultAsync(forceRefresh: true);

            Assert.Equal(uid, response.Uid);
            Assert.Equal(uid, user.Uid);
            Assert.Equal(uid, auth.CurrentUser!.Uid);
            AuthAssertions.NestedCustomClaims(idTokenResult);
        }


        [EmulatorBackendFact]
        public async Task reads_numeric_claims_into_requested_types()
        {
            var auth = CrossFirebaseAuth.Current;
            var uid = IntegrationTestData.UniqueId("numeric-claims");
            var claims = AuthTestPayloads.CreateNumericCustomClaims();
            var requestJson = JsonSerializer.Serialize(new {
                uid,
                claims,
            });
            var response = await CrossFirebaseFunctions.Current
                .GetHttpsCallable("createCustomToken")
                .CallAsync<CustomTokenResponseData>(requestJson);

            await auth.SignOutAsync();
            var user = await auth.SignInWithCustomTokenAsync(response.Token);
            await using var testUser = AuthTestUserScope.TrackCurrentUser(auth);
            var result = await user.GetIdTokenResultAsync(forceRefresh: true);

            // the same claim reads as whichever numeric type is requested
            Assert.Equal((byte) 7, result.GetClaim<byte>("score"));
            Assert.Equal((short) 7, result.GetClaim<short>("score"));
            Assert.Equal(7, result.GetClaim<int>("score"));
            Assert.Equal(7L, result.GetClaim<long>("score"));
            Assert.Equal(7UL, result.GetClaim<ulong>("score"));
            Assert.Equal(7f, result.GetClaim<float>("score"));
            Assert.Equal(7.0, result.GetClaim<double>("score"));
            Assert.Equal(7m, result.GetClaim<decimal>("score"));
            Assert.Equal(7, result.GetClaim<int?>("score"));

            // values at the edge of the requested type
            Assert.Equal(byte.MaxValue, result.GetClaim<byte>("byte_max"));
            Assert.Equal(ByteBackedClaimLevel.Max, result.GetClaim<ByteBackedClaimLevel>("byte_max"));
            Assert.Equal(short.MaxValue, result.GetClaim<short>("short_max"));
            Assert.Equal(int.MaxValue, result.GetClaim<int>("int_max"));
            Assert.Equal(5_000_000_000L, result.GetClaim<long>("above_int"));

            // fractional values truncate toward zero, and decimal keeps the fraction
            Assert.Equal(2, result.GetClaim<int>("positive_fraction"));
            Assert.Equal(-2, result.GetClaim<int>("negative_fraction"));
            Assert.Equal(1.5m, result.GetClaim<decimal>("ratio"));
            Assert.Equal(1.5m, result.GetClaim<decimal?>("ratio"));

            // out-of-range values throw instead of wrapping
            Assert.Throws<OverflowException>(() => result.GetClaim<byte>("above_byte"));
            Assert.Throws<OverflowException>(() => result.GetClaim<ByteBackedClaimLevel>("above_byte"));
            Assert.Throws<OverflowException>(() => result.GetClaim<short>("above_short"));
            Assert.Throws<OverflowException>(() => result.GetClaim<int>("above_int"));
            Assert.Throws<OverflowException>(() => result.GetClaim<uint>("negative"));

            // claims are not coerced between JSON types
            Assert.True(result.GetClaim<bool>("flag"));
            Assert.Throws<InvalidCastException>(() => result.GetClaim<int>("flag"));
            Assert.Equal("123", result.GetClaim<string>("numeric_text"));
            Assert.Throws<InvalidCastException>(() => result.GetClaim<long>("numeric_text"));
            Assert.Throws<InvalidCastException>(() => result.GetClaim<string>("score"));
            Assert.Throws<InvalidCastException>(() => result.GetClaim<bool>("score"));
        }


        [Fact]
        public async Task retrieves_custom_claims()
        {
            var sut = CrossFirebaseAuth.Current;
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(
                sut,
                IntegrationTestUsers.CustomClaimsEmail,
                deleteOnDispose: false);
            var idTokenResult = await user.User.GetIdTokenResultAsync();

            AuthAssertions.NestedCustomClaims(idTokenResult);
        }


        [Fact]
        public async Task exposes_id_token_metadata()
        {
            var sut = CrossFirebaseAuth.Current;
            var email = IntegrationTestData.UniqueEmail("token-metadata");
            await using var user = await AuthTestUserScope.SignInWithEmailAndPasswordAsync(sut, email);

            var idTokenResult = await user.User.GetIdTokenResultAsync();
            var refreshedIdTokenResult = await user.User.GetIdTokenResultAsync(forceRefresh: true);

            Assert.False(string.IsNullOrWhiteSpace(idTokenResult.Token));
            Assert.NotNull(idTokenResult.Claims);
            Assert.NotEmpty(idTokenResult.Claims);
            Assert.NotEqual(default, idTokenResult.AuthDate);
            Assert.NotEqual(default, idTokenResult.IssuedAtDate);
            Assert.NotEqual(default, idTokenResult.ExpirationDate);
            Assert.False(string.IsNullOrWhiteSpace(refreshedIdTokenResult.Token));
            Assert.False(string.IsNullOrWhiteSpace(idTokenResult.SignInProvider));
        }

    }
}
