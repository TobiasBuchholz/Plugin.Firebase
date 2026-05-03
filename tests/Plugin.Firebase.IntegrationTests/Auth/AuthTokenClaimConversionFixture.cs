#if ANDROID
using Java.Util;
using Plugin.Firebase.Auth.Platforms.Android.Extensions;
#elif IOS
using Foundation;
using Plugin.Firebase.Auth.Platforms.iOS.Extensions;
#endif

namespace Plugin.Firebase.IntegrationTests.Auth
{
    [Collection("Sequential")]
    [TestLogging]
    [Microsoft.Maui.Controls.Internals.Preserve(AllMembers = true)]
    public sealed class AuthTokenClaimConversionFixture
    {
#if ANDROID
        [Fact]
        public void converts_android_hashmap_claim_objects_recursively()
        {
            var nestedObject = CreateAndroidNestedObject();

            var converted = Assert.IsAssignableFrom<IDictionary<string, object>>(
                nestedObject.ToObject()
            );
            NestedClaimAssertions.AssertNestedCustomClaim(converted);

            var typedInterface = Assert.IsAssignableFrom<IDictionary<string, object>>(
                nestedObject.ToObject(typeof(IDictionary<string, object>))
            );
            NestedClaimAssertions.AssertNestedCustomClaim(typedInterface);

            var typedConcrete = Assert.IsType<Dictionary<string, object>>(
                nestedObject.ToObject(typeof(Dictionary<string, object>))
            );
            NestedClaimAssertions.AssertNestedCustomClaim(typedConcrete);

            var objectConverted = Assert.IsAssignableFrom<IDictionary<string, object>>(
                nestedObject.ToObject(typeof(object))
            );
            NestedClaimAssertions.AssertNestedCustomClaim(objectConverted);
        }

        [Fact]
        public void converts_android_arraylist_claim_arrays_recursively()
        {
            var nestedArray = CreateAndroidNestedArray();

            var converted = Assert.IsAssignableFrom<IList<object>>(nestedArray.ToObject());
            NestedClaimAssertions.AssertNestedCustomArray(converted);

            var typedInterface = Assert.IsAssignableFrom<IList<object>>(
                nestedArray.ToObject(typeof(IList<object>))
            );
            NestedClaimAssertions.AssertNestedCustomArray(typedInterface);

            var typedConcrete = Assert.IsType<List<object>>(
                nestedArray.ToObject(typeof(List<object>))
            );
            NestedClaimAssertions.AssertNestedCustomArray(typedConcrete);

            var objectConverted = Assert.IsAssignableFrom<IList<object>>(
                nestedArray.ToObject(typeof(object))
            );
            NestedClaimAssertions.AssertNestedCustomArray(objectConverted);
        }

        private static HashMap CreateAndroidNestedObject()
        {
            var nested = new HashMap();
            nested.Put("enabled", true);
            nested.Put("roles", CreateAndroidRoles());
            nested.Put("metadata", CreateAndroidMetadata());
            nested.Put("history", CreateAndroidHistory());
            nested.Put("score", 7);
            nested.Put("ratio", 1.5);
            nested.Put("optional", null);
            return nested;
        }

        private static ArrayList CreateAndroidNestedArray()
        {
            var first = new HashMap();
            first.Put("name", "first");
            var flags = new ArrayList();
            flags.Add(true);
            flags.Add(false);
            first.Put("flags", flags);

            var second = new HashMap();
            second.Put("name", "second");
            second.Put("metadata", CreateAndroidMetadata(includeVersion: false));

            var nestedArray = new ArrayList();
            nestedArray.Add(first);
            nestedArray.Add(second);
            return nestedArray;
        }

        private static ArrayList CreateAndroidRoles()
        {
            var roles = new ArrayList();
            roles.Add("admin");
            roles.Add("tester");
            return roles;
        }

        private static HashMap CreateAndroidMetadata(bool includeVersion = true)
        {
            var metadata = new HashMap();
            metadata.Put("source", "emulator");
            if(includeVersion) {
                metadata.Put("version", 2);
            }
            return metadata;
        }

        private static ArrayList CreateAndroidHistory()
        {
            var created = new HashMap();
            created.Put("action", "created");
            created.Put("count", 1);

            var updated = new HashMap();
            updated.Put("action", "updated");
            updated.Put("count", 2);

            var history = new ArrayList();
            history.Add(created);
            history.Add(updated);
            return history;
        }
#endif

#if IOS
        [Fact]
        public void converts_ios_dictionary_claim_objects_recursively()
        {
            var nestedObject = CreateIosNestedObject();

            var converted = Assert.IsAssignableFrom<IDictionary<string, object>>(
                nestedObject.ToObject()
            );
            NestedClaimAssertions.AssertNestedCustomClaim(converted);

            var typedInterface = Assert.IsAssignableFrom<IDictionary<string, object>>(
                nestedObject.ToObject(typeof(IDictionary<string, object>))
            );
            NestedClaimAssertions.AssertNestedCustomClaim(typedInterface);

            var typedConcrete = Assert.IsType<Dictionary<string, object>>(
                nestedObject.ToObject(typeof(Dictionary<string, object>))
            );
            NestedClaimAssertions.AssertNestedCustomClaim(typedConcrete);

            var objectConverted = Assert.IsAssignableFrom<IDictionary<string, object>>(
                nestedObject.ToObject(typeof(object))
            );
            NestedClaimAssertions.AssertNestedCustomClaim(objectConverted);
        }

        [Fact]
        public void converts_ios_array_claim_arrays_recursively()
        {
            var nestedArray = CreateIosNestedArray();

            var converted = Assert.IsAssignableFrom<IList<object>>(nestedArray.ToObject());
            NestedClaimAssertions.AssertNestedCustomArray(converted);

            var typedInterface = Assert.IsAssignableFrom<IList<object>>(
                nestedArray.ToObject(typeof(IList<object>))
            );
            NestedClaimAssertions.AssertNestedCustomArray(typedInterface);

            var typedConcrete = Assert.IsType<List<object>>(
                nestedArray.ToObject(typeof(List<object>))
            );
            NestedClaimAssertions.AssertNestedCustomArray(typedConcrete);

            var objectConverted = Assert.IsAssignableFrom<IList<object>>(
                nestedArray.ToObject(typeof(object))
            );
            NestedClaimAssertions.AssertNestedCustomArray(objectConverted);
        }

        private static NSDictionary CreateIosNestedObject()
        {
            return NSDictionary.FromObjectsAndKeys(
                new NSObject[] {
                    NSNumber.FromBoolean(true),
                    CreateIosRoles(),
                    CreateIosMetadata(),
                    CreateIosHistory(),
                    NSNumber.FromInt32(7),
                    NSNumber.FromDouble(1.5),
                    NSNull.Null,
                },
                new NSObject[] {
                    new NSString("enabled"),
                    new NSString("roles"),
                    new NSString("metadata"),
                    new NSString("history"),
                    new NSString("score"),
                    new NSString("ratio"),
                    new NSString("optional"),
                }
            );
        }

        private static NSArray CreateIosNestedArray()
        {
            var first = NSDictionary.FromObjectsAndKeys(
                new NSObject[] {
                    new NSString("first"),
                    NSArray.FromNSObjects(NSNumber.FromBoolean(true), NSNumber.FromBoolean(false)),
                },
                new NSObject[] { new NSString("name"), new NSString("flags") }
            );
            var second = NSDictionary.FromObjectsAndKeys(
                new NSObject[] { new NSString("second"), CreateIosMetadata(includeVersion: false) },
                new NSObject[] { new NSString("name"), new NSString("metadata") }
            );

            return NSArray.FromNSObjects(first, second);
        }

        private static NSArray CreateIosRoles()
        {
            return NSArray.FromNSObjects(new NSString("admin"), new NSString("tester"));
        }

        private static NSDictionary CreateIosMetadata(bool includeVersion = true)
        {
            if(!includeVersion) {
                return NSDictionary.FromObjectsAndKeys(
                    new NSObject[] { new NSString("emulator") },
                    new NSObject[] { new NSString("source") }
                );
            }

            return NSDictionary.FromObjectsAndKeys(
                new NSObject[] { new NSString("emulator"), NSNumber.FromInt32(2) },
                new NSObject[] { new NSString("source"), new NSString("version") }
            );
        }

        private static NSArray CreateIosHistory()
        {
            var created = NSDictionary.FromObjectsAndKeys(
                new NSObject[] { new NSString("created"), NSNumber.FromInt32(1) },
                new NSObject[] { new NSString("action"), new NSString("count") }
            );
            var updated = NSDictionary.FromObjectsAndKeys(
                new NSObject[] { new NSString("updated"), NSNumber.FromInt32(2) },
                new NSObject[] { new NSString("action"), new NSString("count") }
            );

            return NSArray.FromNSObjects(created, updated);
        }
#endif
    }
}