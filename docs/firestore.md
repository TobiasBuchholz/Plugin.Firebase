# Firestore

Cloud Firestore is a flexible, scalable database for mobile, web, and server development from Firebase and Google Cloud. Like Firebase Realtime Database, it keeps your data in sync across client apps through realtime listeners and offers offline support for mobile and web so you can build responsive apps that work regardless of network latency or Internet connectivity. Cloud Firestore also offers seamless integration with other Firebase and Google Cloud products, including Cloud Functions.

## Installation
### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.firestore.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Firestore/)

> Install-Package Plugin.Firebase.Firestore

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Cloud Firestore at your project in the [Firebase Console](https://console.firebase.google.com/)

## Usage

To be able to fetch your data from Firestore, create a POCO that implements the `IFirestoreObject` interface and has a public parameterless constructor. Use the `FirestoreDocumentId` and `FirestoreProperty` attributes to hook up its properties. For example:

> Firestore's reflection-based model mapping is not currently compatible with full trimming or Native AOT.

```c#

public sealed class Pokemon : IFirestoreObject
{
    [FirestoreDocumentId]
    public string Id { get; private set; }

    [FirestoreProperty("name")]
    public string Name { get; private set; }

    [FirestoreProperty("weight_in_kg")]
    public double WeightInKg { get; private set; }

    [FirestoreProperty("height_in_cm")]
    public float HeightInCm { get; private set; }

    [FirestoreProperty("sighting_count")]
    public long SightingCount { get; private set; }

    [FirestoreProperty("is_from_first_generation")]
    public bool IsFromFirstGeneration { get; private set; }

    [FirestoreProperty("poke_type")]
    public PokeType PokeType { get; private set; }

    [FirestoreProperty("moves")]
    public IList<string> Moves { get; private set; }
    
    [FirestoreProperty("first_sighting_location")]
    public SightingLocation FirstSightingLocation { get; private set; }

    [FirestoreProperty("items")]
    public IList<SimpleItem> Items { get; private set; }

    [FirestoreProperty("creation_date")]
    public DateTimeOffset CreationDate { get; private set; }

    [FirestoreServerTimestamp("server_timestamp")]
    public DateTimeOffset ServerTimestamp { get; private set; }

    [FirestoreProperty("original_reference")]
    public IDocumentReference OriginalReference { get; private set; }

}
```

This class is represented in firestore like this:

![firestore_poco.png](../art/firestore_poco.png)

Documents can also be read as raw dictionaries when the field set is not known at compile time:
```c#
var snapshot = await CrossFirebaseFirestore.Current
    .GetDocument("pokemons/1")
    .GetDocumentSnapshotAsync<Dictionary<string, object>>();

var data = snapshot.Data;
var name = data["name"] as string;
```

Firestore stores whole numbers as 64-bit integers and fractional numbers as doubles. Reading a
document into a property of a numeric type (`byte`, `sbyte`, `short`, `ushort`, `int`, `uint`,
`long`, `ulong`, `float`, `double`, `decimal`, and their nullable forms) or an enum converts the
stored value with the same rules on both platforms:

- A fractional value read into an integral or enum property is truncated toward zero, as the
  native Android SDK does. A stored `2.7` gives `2`, `-2.7` gives `-2`, and `3.5` gives `3`.
- A value outside the property's range throws an `OverflowException` rather than silently
  wrapping to an unrelated number. As in the native Android SDK, a fractional value is range
  checked as stored, before it is truncated, so `255.5` doesn't fit a `byte` and `-0.5` doesn't
  fit a `uint`. For a `float` property, a double too large for a `float` throws rather than
  becoming infinity.
- A stored NaN or infinity read into an integral, enum or `decimal` property throws an
  `OverflowException`. `float` and `double` properties keep it.
- An enum is range checked against its underlying type, so a value that doesn't fit throws an
  `OverflowException`: `300` or `-1` for a `byte`-backed enum, `2147483648` for an `int`-backed
  one, and `-1` for a `uint`- or `ulong`-backed one. A value in that range that isn't a named
  member still converts, as with a cast in C#, and so does a combination of `[Flags]` members. A
  fractional value is range checked as stored and then truncated, as for integral properties, so
  `2.7` gives the member with value `2` and `255.5` doesn't fit a `byte`-backed enum. Nullable
  enums, enum list elements and enum dictionary values follow the same rules.
- `decimal` and `decimal?` properties are supported.

Numbers are the only values a typed read converts. A stored value whose type doesn't match the
property, list element, dictionary key or dictionary value it is read into throws an
`InvalidCastException` on both platforms. The message names the stored type, the target type and
the field. This follows the native Android SDK, with one difference: the native SDK reads an enum
from the text of a member name, while the plugin writes enums as numbers and reads them only from
numbers. For example, these throw:

- a number read into a `string`, and text read into a number or an enum
- a boolean read into a number, and a number read into a `bool`
- a number or text read into a `DateTime` or `DateTimeOffset`
- a map read into anything other than `object`, a dictionary or a model, such as a `string`, a
  number or a list
- an array read into anything other than `object` or a list, such as a `string`, a number, a
  dictionary or a model
- a field name or map key read into a dictionary whose keys aren't `string` or `object`
- any value read into a `char`, because `char` isn't a Firestore type

Text reads into `string`, booleans into `bool`, numbers into numeric and enum types as described
above, timestamps into `DateTime` and `DateTimeOffset`, geo points into `GeoPoint`, references
into `IDocumentReference`, arrays into lists and maps into dictionaries or models.

```c#
public enum Level : byte { Low = 1, High = 2 }

// "quantity" and "level" hold 300 in Firestore
[FirestoreProperty("quantity")]
public byte Quantity { get; private set; } // throws OverflowException on read

[FirestoreProperty("level")]
public Level SkillLevel { get; private set; } // throws OverflowException on read

// "code" holds the number 123 in Firestore
[FirestoreProperty("code")]
public string Code { get; private set; } // throws InvalidCastException on read
```

Conversion happens when you read `snapshot.Data`, not when the snapshot arrives, so guard that
access inside snapshot listener callbacks if documents may hold values your model can't
represent.

Writing a `decimal` property is not supported on either platform and throws an
`ArgumentException`; store the value as a `double` or `long` instead.

### Further information

Take a look at the [documentation](https://github.com/AdamEssenmacher/GoogleApisForiOSComponents/blob/master/docs/Firebase/CloudFirestore/GettingStarted.md) for the AdamE.Firebase.iOS.CloudFirestore packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseFirestore.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IFirebaseFirestore.cs)
- [src/.../ICollectionReference.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/ICollectionReference.cs)
- [src/.../IDocumentReference.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IDocumentReference.cs)
- [src/.../IQuery.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IQuery.cs)
- [src/.../ITransaction.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/ITransaction.cs)
- [src/.../IWriteBatch.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IWriteBatch.cs)
- [tests/.../FirestoreFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/Firestore/FirestoreFixture.cs)

## Next-release migration notes

Typed reads of numbers now follow the same rules on both platforms (see [Usage](#usage)). Compared
with Plugin.Firebase.Firestore 4.0.0:

- On iOS, a value outside the range of an integral property wrapped silently, so a stored `300`
  read into a `byte` gave `44`. It now throws `OverflowException`. `decimal` properties, which
  were left at `0` (`null` for `decimal?`), now read the stored value.
- On Android, `int` and `int?` properties and `IList<int>` and `IList<long>` elements rounded
  fractional values (`Convert.ToInt32`, `Convert.ToInt64`), so `2.5` gave `2` and `3.5` gave
  `4`. They now truncate, so `3.5` gives `3`.
- A fractional value read into any integral or enum property truncates toward zero on both
  platforms, after a range check on the stored value. On iOS, a value just outside the range,
  like `255.5` for a `byte`, read as `255`; it now throws `OverflowException`. On Android, a
  fractional value read into an integral property other than `int` threw `ArgumentException`;
  it now converts.
- Enum properties are range checked against their underlying type on both platforms, so a value
  that doesn't fit throws `OverflowException`. Before, iOS wrapped it silently, so a stored `300`
  read into a `byte`-backed enum gave `(Level) 44`. Android threw `ArgumentException` for enums
  that aren't `long`-backed, whether in a model property, a list or a dictionary; values in range
  now read.
- Android wrote `uint`-backed enum values above `int.MaxValue`, and `ulong`-backed values above
  `long.MaxValue`, as negative numbers when they went through update, list, query or
  transaction-result writes. This release fixes those writes but can't repair values already
  stored, which don't fit the enum and now fail to read on both platforms. To recover, read the
  document as a `Dictionary<string, object>`, convert the stored `long` with
  `unchecked((uint) value)` or `unchecked((ulong) value)`, and write the field back.
- iOS read those wrapped values back as the original member, because its enum reads wrapped too,
  so documents written on Android round-tripped there. They now throw on iOS as well. Writing a
  field back as described above stores the actual value only when this release also includes the
  Android enum write fix (#747). Write a list field back whole: `FieldValue.ArrayUnion` adds the
  corrected element next to the wrapped one instead of replacing it, and `FieldValue.ArrayRemove`
  with the enum value doesn't match the wrapped element.
- A stored NaN or infinity read into an integral, enum or `decimal` property throws
  `OverflowException`, and so does a double too large for a `float` property.
- On iOS, `char` properties read the stored number through a byte, so values above 255 wrapped.
  `char` isn't a Firestore type, so reading any value into a `char` now throws
  `InvalidCastException` on both platforms.

Typed reads no longer convert between types other than numbers (see [Usage](#usage)). A value of
the wrong type now throws `InvalidCastException` on both platforms, in model properties, list
elements, dictionary keys and values, and `RunTransactionAsync<T>` results. Compared with
Plugin.Firebase.Firestore 4.0.0:

- On iOS, a number read into a `string` gave `null`, and a number read into a `DateTime` or another
  type that isn't numeric gave the type's default value. Booleans and numbers converted into each
  other, so `true` read into an `int` gave `1`, and a number read into a `bool` gave `true` or
  `false`. These now throw.
- On iOS, other mismatches, such as text read into an `int` or an enum, or a timestamp read into a
  `string`, threw `ArgumentException`. They now throw `InvalidCastException`.
- On Android, a value of the wrong type usually threw `ArgumentException`, which is now
  `InvalidCastException`. A few reads converted instead, using the device culture: `int`, `int?`
  and `float` properties parsed text and read booleans as `1` and `0`, and elements of `string`,
  `int`, `long`, `float`, `double`, `decimal` and `bool` lists were converted with
  `Convert.ToString`, `Convert.ToInt32` and so on. These now throw.
- On both platforms, a map read into a number, `bool`, enum, `char`, `DateTime` or
  `DateTimeOffset` gave the type's default value, and into a `List<T>` it gave an empty list. Into
  a `string`, an interface such as `IList<T>`, an array or a `GeoPoint` it threw
  `MissingMethodException`. An array read into a single value, a model or a dictionary threw an
  exception that didn't name the mismatch. These now throw `InvalidCastException`.
- `RunTransactionAsync<T>` converts the update function's result back to `T` with the same rules,
  so a result that doesn't come back as `T` throws `InvalidCastException` with the same message.
- Earlier iOS versions stored the values of `byte`- and `sbyte`-backed enums as booleans, so every
  value other than `0` read back as `1`. Reading such a field into the enum now throws
  `InvalidCastException`. Rewrite the affected fields with this release, which stores the actual
  value (#747).
- Unreleased builds of the `development` branch briefly converted these values on Android with
  `Convert.ChangeType`: any value read into a `string` became text, and text read into a number,
  `bool`, `char` or `DateTime` was parsed. No release did this.

If your documents hold values of mixed types, read them into a property of type `object`, or as
`Dictionary<string, object>`, and convert them yourself.

## Release notes
- Version 3.1.3
  - Added the handling of a FieldValue into a Hashmap correctly + Prevent crashing when a list on the server has a null as an entry (PR #437)
- Version 3.1.2
  - Using AdamE.Firebase.iOS.* minimum version 11
  - Use JavaObject constructor instead of addding individual items
- Version 3.1.1
  - ToNSObject and ToJavaObject handle null inputs (PR #352)
  - Fixing IWriteBatch.UpdateData issue where .NET objects were not being converted to NSObjects (PR #351)
- Version 3.1.0
  - Update to .net8
- Version 3.0.0
  - Swapped Xamarin.Firebase.iOS.Firestore (native SDK 8.10.0) for AdamE.Firebase.iOS.Firestore (native SDK 10.24.0)
- Version 2.0.7
  - Fixed missing DateTime in firestore (PR #280)
  - Fix simple number arrays on firestore android (issue #276)
- Version 2.0.6
  - Support int properties on IFirestoreObjects on Android (PR #247)
  - Firestore plugin converts NSNumbers to nullable .NET types (e.g. float?, int?, long?) (PR #250)
- Version 2.0.5
  - Bumped up Xamarin.Firebase.Firestore package to version 124.8.1.1
- Version 2.0.4
  - Firestore CollectionReference inherits Query (PR #205)
  - Adding GetCollectionGroup to FirebaseFirestore (PR #207)
  - Adding synchronous versions of WriteBatch.Commit() (PR #208)
  - Added the Parent property to CollectionReference. (PR #209)
- Version 2.0.3
  - Use `Debug.WriteLine()` in object extensions (issue #174)
- Version 2.0.2
  - re-add tohashmap extension for firestore setdataasync method when data is a dictionary of objects
  - fix firestore setdataasync for ios
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm
