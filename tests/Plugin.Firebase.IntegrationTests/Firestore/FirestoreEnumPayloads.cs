using JetBrains.Annotations;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

// Enums with members at the edges of their underlying type, each read through a document that holds
// a single value, so every stored value is asserted on its own.
internal enum EdgeByteRating : byte
{
    Min = byte.MinValue,
    Max = byte.MaxValue
}

internal enum EdgeIntRating : int
{
    Min = int.MinValue,
    Max = int.MaxValue
}

internal enum EdgeUIntRating : uint
{
    Min = uint.MinValue,
    Max = uint.MaxValue
}

internal enum EdgeLongRating : long
{
    Min = long.MinValue,
    Max = long.MaxValue
}

internal enum EdgeULongRating : ulong
{
    Min = ulong.MinValue,
    Max = ulong.MaxValue
}

// Admin and All only fit a uint: a cast to int turns them into negative numbers.
[Flags]
internal enum UIntPermissions : uint
{
    None = 0,
    Read = 1,
    Write = 2,
    Admin = 0x80000000,
    All = 0xFFFFFFFF
}

[Preserve(AllMembers = true)]
internal sealed class EdgeByteRatingDocument : IFirestoreObject
{
    [FirestoreProperty(SingleNumberDocument.Field)]
    public EdgeByteRating Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class EdgeIntRatingDocument : IFirestoreObject
{
    [FirestoreProperty(SingleNumberDocument.Field)]
    public EdgeIntRating Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class EdgeUIntRatingDocument : IFirestoreObject
{
    [FirestoreProperty(SingleNumberDocument.Field)]
    public EdgeUIntRating Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class EdgeLongRatingDocument : IFirestoreObject
{
    [FirestoreProperty(SingleNumberDocument.Field)]
    public EdgeLongRating Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class EdgeULongRatingDocument : IFirestoreObject
{
    [FirestoreProperty(SingleNumberDocument.Field)]
    public EdgeULongRating Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class UIntPermissionsDocument : IFirestoreObject
{
    [FirestoreProperty(SingleNumberDocument.Field)]
    public UIntPermissions Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class NullableRatingDocument : IFirestoreObject
{
    [FirestoreProperty(SingleNumberDocument.Field)]
    public NumericRating? Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class RatingListDocument : IFirestoreObject
{
    public const string Field = "ratings";

    [FirestoreProperty(Field)]
    public IList<NumericRating>? Ratings { get; [UsedImplicitly] private set; }
}