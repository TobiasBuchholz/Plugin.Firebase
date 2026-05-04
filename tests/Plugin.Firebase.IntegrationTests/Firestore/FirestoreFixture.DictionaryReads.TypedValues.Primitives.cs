using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task gets_document_data_as_strongly_typed_dictionaries()
    {
        var sut = CrossFirebaseFirestore.Current;

        var stringDocument = GetTestingDocument(sut, "typed-string-map");
        await stringDocument.SetDataAsync(new Dictionary<object, object?> {
            { "alpha", "one" },
            { "beta", "two" }
        });
        var strings = (await stringDocument.GetDocumentSnapshotAsync<Dictionary<string, string>>()).Data!;
        Assert.Equal("one", strings["alpha"]);
        Assert.Equal("two", strings["beta"]);

        var boolDocument = GetTestingDocument(sut, "typed-bool-map");
        await boolDocument.SetDataAsync(new Dictionary<object, object?> {
            { "enabled", true },
            { "archived", false }
        });
        var bools = (await boolDocument.GetDocumentSnapshotAsync<Dictionary<object, bool>>()).Data!;
        Assert.All(bools.Keys, key => Assert.IsType<string>(key));
        Assert.True(bools["enabled"]);
        Assert.False(bools["archived"]);

        var longDocument = GetTestingDocument(sut, "typed-long-map");
        await longDocument.SetDataAsync(new Dictionary<object, object?> {
            { "one", 1L },
            { "two", 2 }
        });
        var longs = (await longDocument.GetDocumentSnapshotAsync<IDictionary<string, long>>()).Data!;
        Assert.Equal(1L, longs["one"]);
        Assert.Equal(2L, longs["two"]);

        var intDocument = GetTestingDocument(sut, "typed-int-map");
        await intDocument.SetDataAsync(new Dictionary<object, object?> {
            { "one", 1L },
            { "two", 2 }
        });
        var ints = (await intDocument.GetDocumentSnapshotAsync<Dictionary<string, int>>()).Data!;
        Assert.Equal(1, ints["one"]);
        Assert.Equal(2, ints["two"]);

        var doubleDocument = GetTestingDocument(sut, "typed-double-map");
        await doubleDocument.SetDataAsync(new Dictionary<object, object?> {
            { "half", 0.5 },
            { "whole", 2L }
        });
        var doubles = (await doubleDocument.GetDocumentSnapshotAsync<Dictionary<string, double>>()).Data!;
        Assert.Equal(0.5, doubles["half"]);
        Assert.Equal(2.0, doubles["whole"]);

        var floatDocument = GetTestingDocument(sut, "typed-float-map");
        await floatDocument.SetDataAsync(new Dictionary<object, object?> {
            { "half", 0.5 },
            { "whole", 2L }
        });
        var floats = (await floatDocument.GetDocumentSnapshotAsync<Dictionary<string, float>>()).Data!;
        Assert.Equal(0.5f, floats["half"]);
        Assert.Equal(2.0f, floats["whole"]);

        var enumDocument = GetTestingDocument(sut, "typed-enum-map");
        await enumDocument.SetDataAsync(new Dictionary<object, object?> {
            { "fire", PokeType.Fire },
            { "water", PokeType.Water }
        });
        var enums = (await enumDocument.GetDocumentSnapshotAsync<Dictionary<string, PokeType>>()).Data!;
        Assert.Equal(PokeType.Fire, enums["fire"]);
        Assert.Equal(PokeType.Water, enums["water"]);
    }

    [Fact]
    public async Task gets_document_data_as_additional_numeric_dictionaries()
    {
        var sut = CrossFirebaseFirestore.Current;

        var byteDocument = GetTestingDocument(sut, "typed-byte-map");
        await byteDocument.SetDataAsync(new Dictionary<object, object?> {
            { "min", 0L },
            { "max", 255L }
        });
        var bytes = (await byteDocument.GetDocumentSnapshotAsync<Dictionary<string, byte>>()).Data!;
        Assert.Equal((byte) 0, bytes["min"]);
        Assert.Equal(byte.MaxValue, bytes["max"]);

        var sbyteDocument = GetTestingDocument(sut, "typed-sbyte-map");
        await sbyteDocument.SetDataAsync(new Dictionary<object, object?> {
            { "min", -128L },
            { "max", 127L }
        });
        var sbytes = (await sbyteDocument.GetDocumentSnapshotAsync<Dictionary<string, sbyte>>()).Data!;
        Assert.Equal(sbyte.MinValue, sbytes["min"]);
        Assert.Equal(sbyte.MaxValue, sbytes["max"]);

        var shortDocument = GetTestingDocument(sut, "typed-short-map");
        await shortDocument.SetDataAsync(new Dictionary<object, object?> {
            { "min", -32768L },
            { "max", 32767L }
        });
        var shorts = (await shortDocument.GetDocumentSnapshotAsync<Dictionary<string, short>>()).Data!;
        Assert.Equal(short.MinValue, shorts["min"]);
        Assert.Equal(short.MaxValue, shorts["max"]);

        var ushortDocument = GetTestingDocument(sut, "typed-ushort-map");
        await ushortDocument.SetDataAsync(new Dictionary<object, object?> {
            { "min", 0L },
            { "max", 65535L }
        });
        var ushorts = (await ushortDocument.GetDocumentSnapshotAsync<Dictionary<string, ushort>>()).Data!;
        Assert.Equal((ushort) 0, ushorts["min"]);
        Assert.Equal(ushort.MaxValue, ushorts["max"]);

        var uintDocument = GetTestingDocument(sut, "typed-uint-map");
        await uintDocument.SetDataAsync(new Dictionary<object, object?> {
            { "min", 0L },
            { "max", 4294967295L }
        });
        var uints = (await uintDocument.GetDocumentSnapshotAsync<Dictionary<string, uint>>()).Data!;
        Assert.Equal(0U, uints["min"]);
        Assert.Equal(uint.MaxValue, uints["max"]);

        var ulongDocument = GetTestingDocument(sut, "typed-ulong-map");
        await ulongDocument.SetDataAsync(new Dictionary<object, object?> {
            { "zero", 0L },
            { "value", 9223372036854775807L }
        });
        var ulongs = (await ulongDocument.GetDocumentSnapshotAsync<Dictionary<string, ulong>>()).Data!;
        Assert.Equal(0UL, ulongs["zero"]);
        Assert.Equal(9223372036854775807UL, ulongs["value"]);

        var nullableDocument = GetTestingDocument(sut, "typed-nullable-int-map");
        await nullableDocument.SetDataAsync(new Dictionary<object, object?> {
            { "present", 123L },
            { "missing", null }
        });
        var nullableInts = (await nullableDocument.GetDocumentSnapshotAsync<Dictionary<string, int?>>()).Data!;
        Assert.Equal(123, nullableInts["present"]);
        Assert.Null(nullableInts["missing"]);
    }
}