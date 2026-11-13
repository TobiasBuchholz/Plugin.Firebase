using Plugin.Firebase.Firestore;
using Plugin.Firebase.Firestore.Internal;

namespace Plugin.Firebase.UnitTests.Firestore;

public class FirestoreValueConversionTests
{
    public enum Rating : byte { None, Low, High }

    public sealed class Model { }

    public abstract class AbstractModel { }

    public struct StructModel : IFirestoreObject { }

    public struct PlainStruct { }

    private static readonly DateTime Utc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTimeOffset Offset = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // Firestore text arrives as string, booleans as bool, and numbers as long or double on Android
    // and as their widest primitive on iOS. Timestamps arrive as DateTime or DateTimeOffset.
    public static TheoryData<object, Type, object> Matches => new() {
        // A value that already has the target type, or a type it is assignable to.
        { "text", typeof(string), "text" },
        { "", typeof(string), "" },
        { "2.5", typeof(string), "2.5" },
        { "text", typeof(object), "text" },
        { "text", typeof(IComparable), "text" },
        { true, typeof(bool), true },
        { false, typeof(bool?), false },
        { true, typeof(object), true },
        { 2L, typeof(long), 2L },
        { 2L, typeof(long?), 2L },
        { 2L, typeof(object), 2L },
        { 2.5, typeof(double), 2.5 },
        { Utc, typeof(DateTime), Utc },
        { Offset, typeof(DateTimeOffset), Offset },
        { Offset, typeof(DateTimeOffset?), Offset },

        // A number read into a numeric or enum type follows FirestoreNumericConversion.
        { 2L, typeof(int), 2 },
        { 2L, typeof(byte), (byte) 2 },
        { 2L, typeof(double), 2.0 },
        { 2.7, typeof(int), 2 },
        { 2.5, typeof(float), 2.5f },
        { 2.5, typeof(decimal), 2.5m },
        { 2L, typeof(int?), 2 },
        { 2L, typeof(Rating), Rating.High },
        { 2.7, typeof(Rating?), Rating.High },
        { ulong.MaxValue, typeof(ulong), ulong.MaxValue }
    };

    public static TheoryData<object, Type> Mismatches => new() {
        // A number read into text.
        { 123L, typeof(string) },
        { 0L, typeof(string) },
        { 2.5, typeof(string) },
        { 123UL, typeof(string) },

        // Text read into a number or an enum.
        { "123", typeof(int) },
        { "123", typeof(long) },
        { "255", typeof(byte) },
        { "2.5", typeof(double) },
        { "2.5", typeof(float) },
        { "2.5", typeof(decimal) },
        { "123", typeof(int?) },
        { "2", typeof(Rating) },
        { "High", typeof(Rating) },
        { "High", typeof(Rating?) },

        // A boolean read into a number or an enum, and a number read into a bool.
        { true, typeof(int) },
        { false, typeof(long) },
        { true, typeof(double) },
        { true, typeof(decimal) },
        { true, typeof(int?) },
        { true, typeof(Rating) },
        { 1L, typeof(bool) },
        { 0L, typeof(bool) },
        { 1.0, typeof(bool) },
        { 1L, typeof(bool?) },

        // Text and booleans read into each other.
        { "true", typeof(bool) },
        { true, typeof(string) },

        // char isn't a Firestore type, so nothing reads into one.
        { 65L, typeof(char) },
        { 65.5, typeof(char) },
        { "A", typeof(char) },
        { true, typeof(char) },
        { 65L, typeof(char?) },

        // Numbers and text read into dates and other types.
        { 1767225600000L, typeof(DateTime) },
        { 1767225600.5, typeof(DateTimeOffset) },
        { 1L, typeof(DateTime?) },
        { 1L, typeof(TimeSpan) },
        { 1L, typeof(Guid) },
        { "2026-01-01T00:00:00Z", typeof(DateTime) },
        { "2026-01-01T00:00:00Z", typeof(DateTimeOffset) },
        { "00:01:00", typeof(TimeSpan) },
        { "3f2504e0-4f89-11d3-9a0c-0305e82c3301", typeof(Guid) },
        { 1L, typeof(Model) },
        { "text", typeof(Model) },
        { true, typeof(GeoPoint) },
        { 1L, typeof(IList<long>) },
        { "text", typeof(IList<char>) },
        { 1L, typeof(Dictionary<string, object>) },

        // Values the platforms unwrap from timestamps, geo points, arrays and maps.
        { Offset, typeof(string) },
        { Offset, typeof(long) },
        { Utc, typeof(string) },
        { new GeoPoint(1, 2), typeof(string) },
        { new List<object?> { 1L }, typeof(string) },
        { new List<object?> { 1L }, typeof(long) },
        { new Dictionary<string, object?>(), typeof(int) }
    };

    // A map reads into object, the dictionary types the platforms create, or a model.
    public static TheoryData<Type> MapTargets => new() {
        typeof(object),
        typeof(Dictionary<string, object>),
        typeof(IDictionary<string, long>),
        typeof(Dictionary<string, Model>),
        typeof(Model),
        typeof(StructModel),
        typeof(StructModel?)
    };

    public static TheoryData<Type> NonMapTargets => new() {
        // Single values.
        typeof(string),
        typeof(bool),
        typeof(char),
        typeof(byte),
        typeof(int),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(Rating),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(Guid),
        typeof(int?),
        typeof(Rating?),
        typeof(DateTimeOffset?),
        typeof(GeoPoint),

        // Lists and arrays, which a map used to fill as an empty list or fail to create.
        typeof(List<string>),
        typeof(IList<string>),
        typeof(string[]),
        typeof(HashSet<string>),

        // Dictionary types the platforms don't create, interfaces and abstract classes, and structs
        // that aren't models.
        typeof(IReadOnlyDictionary<string, object>),
        typeof(SortedDictionary<string, object>),
        typeof(IFirestoreObject),
        typeof(AbstractModel),
        typeof(PlainStruct),
        typeof(KeyValuePair<string, object>)
    };

    // An array reads into object or a type a List<T> can be assigned to. An array target passes, so
    // the platform can say that lists are supported instead.
    public static TheoryData<Type> ArrayTargets => new() {
        typeof(object),
        typeof(List<long>),
        typeof(IList<string>),
        typeof(IList<Model>),
        typeof(ICollection<object>),
        typeof(IEnumerable<string>),
        typeof(IReadOnlyList<int>),
        typeof(IReadOnlyCollection<int>),
        typeof(string[])
    };

    public static TheoryData<Type> NonArrayTargets => new() {
        typeof(string),
        typeof(bool),
        typeof(char),
        typeof(int),
        typeof(double),
        typeof(decimal),
        typeof(Rating),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(int?),
        typeof(GeoPoint),
        typeof(Model),
        typeof(StructModel),
        typeof(Dictionary<string, object>),
        typeof(IDictionary<string, long>),
        typeof(HashSet<string>),
        typeof(System.Collections.ObjectModel.Collection<string>),
        typeof(Lazy<string>)
    };

    [Theory]
    [MemberData(nameof(Matches))]
    public void converts_values_that_match_the_target(object value, Type targetType, object expected)
    {
        var result = FirestoreValueConversion.ToTargetType(value, targetType);

        Assert.NotNull(result);
        Assert.Equal(expected.GetType(), result.GetType());
        Assert.Equal(expected, result);
    }

    [Fact]
    public void returns_values_of_the_target_type_unchanged()
    {
        var geoPoint = new GeoPoint(1, 2);
        var list = new List<string> { "a" };
        var dictionary = new Dictionary<string, object> { { "a", 1L } };
        var model = new Model();

        Assert.Same(geoPoint, FirestoreValueConversion.ToTargetType(geoPoint, typeof(GeoPoint)));
        Assert.Same(list, FirestoreValueConversion.ToTargetType(list, typeof(IList<string>)));
        Assert.Same(dictionary, FirestoreValueConversion.ToTargetType(dictionary, typeof(IDictionary<string, object>)));
        Assert.Same(model, FirestoreValueConversion.ToTargetType(model, typeof(Model)));
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(string))]
    [InlineData(typeof(char))]
    [InlineData(typeof(Rating?))]
    public void passes_null_through(Type targetType)
    {
        Assert.Null(FirestoreValueConversion.ToTargetType(null, targetType));
    }

    [Theory]
    [InlineData(2L)]
    [InlineData("text")]
    [InlineData(true)]
    public void returns_the_value_unchanged_for_an_untyped_read(object value)
    {
        Assert.Same(value, FirestoreValueConversion.ToTargetType(value, null));
    }

    [Theory]
    [MemberData(nameof(Mismatches))]
    public void throws_when_the_value_does_not_match_the_target(object value, Type targetType)
    {
        Assert.Throws<InvalidCastException>(() => FirestoreValueConversion.ToTargetType(value, targetType));
    }

    [Fact]
    public void still_range_checks_numbers_read_into_numeric_targets()
    {
        Assert.Throws<OverflowException>(() => FirestoreValueConversion.ToTargetType(300L, typeof(byte)));
    }

    [Fact]
    public void mismatch_message_names_the_source_type_the_target_type_and_the_field()
    {
        var exception = Assert.Throws<InvalidCastException>(
            () => FirestoreValueConversion.ToTargetType(123L, typeof(string), "code"));

        Assert.Equal("Can't convert a value of type System.Int64 read from Firestore field 'code' to System.String.", exception.Message);
    }

    [Fact]
    public void mismatch_message_names_the_underlying_type_of_a_nullable_target()
    {
        var exception = Assert.Throws<InvalidCastException>(
            () => FirestoreValueConversion.ToTargetType("2", typeof(Rating?), "rating"));

        Assert.Contains(typeof(string).FullName!, exception.Message);
        Assert.Contains(typeof(Rating).FullName!, exception.Message);
        Assert.Contains("'rating'", exception.Message);
    }

    [Fact]
    public void mismatch_message_leaves_out_an_unknown_field()
    {
        var exception = Assert.Throws<InvalidCastException>(
            () => FirestoreValueConversion.ToTargetType(true, typeof(int)));

        Assert.Equal("Can't convert a value of type System.Boolean read from Firestore to System.Int32.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(MapTargets))]
    public void accepts_maps_read_into_objects_dictionaries_and_models(Type targetType)
    {
        FirestoreValueConversion.EnsureMapTarget(targetType, "field");
    }

    [Theory]
    [MemberData(nameof(NonMapTargets))]
    public void throws_when_a_map_is_read_into_any_other_target(Type targetType)
    {
        Assert.Throws<InvalidCastException>(() => FirestoreValueConversion.EnsureMapTarget(targetType, "field"));
    }

    [Theory]
    [MemberData(nameof(ArrayTargets))]
    public void accepts_arrays_read_into_objects_and_lists(Type targetType)
    {
        FirestoreValueConversion.EnsureArrayTarget(targetType, "field");
    }

    [Theory]
    [MemberData(nameof(NonArrayTargets))]
    public void throws_when_an_array_is_read_into_any_other_target(Type targetType)
    {
        Assert.Throws<InvalidCastException>(() => FirestoreValueConversion.EnsureArrayTarget(targetType, "field"));
    }

    [Fact]
    public void accepts_maps_and_arrays_in_an_untyped_read()
    {
        FirestoreValueConversion.EnsureMapTarget(null);
        FirestoreValueConversion.EnsureArrayTarget(null);
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(object))]
    [InlineData(typeof(IComparable))]
    public void converts_map_keys_to_types_that_hold_text(Type keyType)
    {
        Assert.Equal("name", FirestoreValueConversion.ToKeyType("name", keyType));
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(long?))]
    [InlineData(typeof(Rating))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(char))]
    public void throws_when_a_map_key_is_read_into_a_type_that_does_not_hold_text(Type keyType)
    {
        Assert.Throws<InvalidCastException>(() => FirestoreValueConversion.ToKeyType("1", keyType));
    }

    [Fact]
    public void key_mismatch_message_names_the_key_and_the_key_type()
    {
        var exception = Assert.Throws<InvalidCastException>(
            () => FirestoreValueConversion.ToKeyType("count", typeof(int)));

        Assert.Equal("Can't convert the map key 'count' read from Firestore to System.Int32.", exception.Message);
    }

    [Fact]
    public void collection_mismatch_messages_name_the_firestore_type_the_target_type_and_the_field()
    {
        var map = Assert.Throws<InvalidCastException>(() => FirestoreValueConversion.EnsureMapTarget(typeof(int), "count"));
        var array = Assert.Throws<InvalidCastException>(() => FirestoreValueConversion.EnsureArrayTarget(typeof(string), "name"));

        Assert.Equal("Can't convert a map read from Firestore field 'count' to System.Int32.", map.Message);
        Assert.Equal("Can't convert an array read from Firestore field 'name' to System.String.", array.Message);
    }
}