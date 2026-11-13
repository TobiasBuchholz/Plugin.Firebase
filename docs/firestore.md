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
> For the other iOS Native AOT requirements, see [iOS NativeAOT (`PublishAot`)](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/README.md#ios-nativeaot-publishaot) in the README.

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

A stored array can contain `null` entries. Reading it into a list property keeps them as `null` when the element type is a reference type such as `string` or `object`, or a nullable value type such as `int?`. A list of a non-nullable value type such as `IList<int>` can't hold `null`, so reading an array with a `null` entry into it throws an `ArgumentNullException` when you read `snapshot.Data`. Both platforms behave the same way.

### Writing numbers

Firestore stores every number as a signed 64-bit integer or a double. Android and iOS convert .NET numbers the
same way wherever they are written: model properties, dictionary values, list elements, `FieldValue.ArrayUnion` and
`ArrayRemove` elements and query operands.

| .NET type | Stored as |
|---|---|
| `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long` | 64-bit integer |
| `ulong` | 64-bit integer. Values above `long.MaxValue` throw `ArgumentException`. |
| `float`, `double` | double. A `float` is widened, so `0.1f` is stored as `0.10000000149011612`. |
| enum | Its underlying value, under the rules above. |
| `decimal` | Not supported. Throws `ArgumentException`, because Firestore has no decimal type. |
| `char` | Not supported. Throws `ArgumentException`, because Firestore has no character type. |
| `nint`, `nuint`, `Half`, `Int128`, `UInt128`, `BigInteger` | Not supported. Throws `ArgumentException`. |
| `byte[]`, `sbyte[]` | Not supported. Throws `ArgumentException`, because this API doesn't map Firestore bytes (Blob) values. |

Nullable forms follow the rules of their underlying type, and `null` is stored as a Firestore null. To store an
unsupported value, convert it explicitly first, for example `(double) price`.

Documents can also be read as raw dictionaries when the field set is not known at compile time:
```c#
var snapshot = await CrossFirebaseFirestore.Current
    .GetDocument("pokemons/1")
    .GetDocumentSnapshotAsync<Dictionary<string, object>>();

var data = snapshot.Data;
var name = data["name"] as string;
```

A snapshot converts its data the first time `Data` is read. For a class model, later reads of the same snapshot return that same instance, so changes you make to it stay visible through the snapshot, but they are not written to Firestore. A failed conversion isn't kept, so the next read of `Data` tries again. Query snapshots likewise return the same document snapshots each time you enumerate `Documents`, `DocumentChanges` or `GetDocumentChanges(...)`. Each of those lists has its own snapshots, so a change made through `Data` in one list isn't visible in another.

### Error handling

Every failure that the native Firestore SDK reports as a `com.google.firebase.FirebaseException` (Android, which
includes `FirebaseFirestoreException`) or an `NSError` (iOS) is surfaced as `CrossPlatformFirebaseFirestoreException` (`Plugin.Firebase.Core.Exceptions`). That covers the tasks
returned by `IFirebaseFirestore`, `IDocumentReference`, `ICollectionReference`, `IQuery` and `IWriteBatch`, the
exception thrown by `ITransaction.GetDocument`, and the error passed to the `onError` callback of
`AddSnapshotListener`. The type derives from `CrossPlatformFirebaseException`, the base that
`CrossPlatformFirebaseAuthException` shares, and from `FirebaseException`. It keeps the native exception as
`InnerException` and passes the native metadata through:

| Property | Android | iOS |
| --- | --- | --- |
| `NativeExceptionTypeName` | Java canonical class name, e.g. `com.google.firebase.firestore.FirebaseFirestoreException` | `NSError` |
| `NativeErrorCode` | `FirebaseFirestoreException.Code` name, e.g. `INVALID_ARGUMENT`; `null` for other Firebase exceptions | `FirestoreErrorCode` name, e.g. `InvalidArgument`; for other domains or unnamed codes, the number formatted with the invariant culture |
| `NativeErrorDomain` | `null` | `NSError` domain, e.g. `FIRFirestoreErrorDomain` |
| `NativeErrorMessage` | The exception message | The `NSError` localized description |

Only native Firebase failures are wrapped. Argument validation by the native SDKs (for example
`Java.Lang.IllegalArgumentException` on Android, or an Objective-C exception on iOS) and managed exceptions, such
as data conversion errors, propagate unchanged.

```c#
try {
    await CrossFirebaseFirestore.Current.GetDocument("pokemons/1").UpdateDataAsync(("name", "Bulbasaur"));
} catch(CrossPlatformFirebaseFirestoreException e) {
    Console.WriteLine($"{e.NativeErrorDomain} {e.NativeErrorCode}: {e.NativeErrorMessage}");
}
```

Transactions follow the same rules, with one addition. If the update function passed to `RunTransactionAsync`
throws, the transaction is aborted without committing and `RunTransactionAsync` throws a `FirebaseException` whose
`InnerException` is the original exception, on both platforms. A `CrossPlatformFirebaseFirestoreException` that the
plugin raised for a native Firestore error, for example from a failed `ITransaction.GetDocument`, is instead handed
back to the native SDK when the update function lets it propagate. Its `InnerException` is the native
`FirebaseFirestoreException` on Android, or the `NSErrorException` for a `FIRFirestoreErrorDomain` error on iOS. The
native SDK applies its transaction retry rules to that error on both platforms. If the error is retryable, such as
`ABORTED` because a document changed between two reads, the transaction is retried and the update function runs
again. Otherwise, or once no retries are left, `RunTransactionAsync` throws a
`CrossPlatformFirebaseFirestoreException` for the same native error; it isn't wrapped again. On Android the same
applies to a native `FirebaseFirestoreException` that the update function throws directly.

Only the exception that the update function throws is checked. If you catch a failed read and throw your own
exception, even one that keeps the Firestore exception as `InnerException`, the transaction is aborted without a retry
and `RunTransactionAsync` throws a `FirebaseException` whose `InnerException` is your exception. A
`CrossPlatformFirebaseFirestoreException` that doesn't carry a native Firestore error, for example one you construct
yourself, also aborts the transaction without a retry, and `RunTransactionAsync` throws it unchanged.

The update function must be synchronous: the native SDKs commit the transaction as soon as it returns, so an `async`
update function would commit only the writes made before its first `await`. `RunTransactionAsync` throws an
`ArgumentException` before the transaction starts when `TResult` is `Task`, `ValueTask` or `ValueTask<T>`, which the
compiler infers for an `async` lambda. If the update function returns a task through another `TResult`, such as
`object`, the transaction is aborted without committing and `RunTransactionAsync` throws that `ArgumentException`.
The code after the first `await` of such an update function still runs, outside the transaction: its writes are
silently discarded, and an exception it throws is never observed. Custom task-like types and awaitables such as
`ConfiguredTaskAwaitable` aren't detected, so don't return those either. Do asynchronous work before or after the
transaction.

`RunTransactionAsync` returns the value that the update function returned in the attempt that committed, as-is: the
same instance, never converted to or from a Firestore value. A lazily evaluated result, such as a LINQ query over
transactional reads, is enumerated only after the transaction has finished, as with the native SDKs, so materialize it
inside the update function, for example with `ToList()`, if it depends on the transaction.

### Writing dates

Firestore stores dates as instants. A `DateTimeOffset` always identifies one. A `DateTime` must have `DateTimeKind.Utc` or `DateTimeKind.Local` and is stored as its UTC instant; writing one with `DateTimeKind.Unspecified`, such as `default(DateTime)`, throws `ArgumentException`. Use `DateTime.SpecifyKind` to say which time zone such a value is in. Dates are read back in UTC.

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

On Android, `DateTime` and `DateTimeOffset` values now convert directly to and from the native Firestore `Timestamp` instead of going through `java.util.Date`, so they keep the microseconds that Firestore stores. The native SDKs drop anything smaller than a microsecond. These notes cover Android only; iOS precision is addressed separately.

- Writes keep microseconds. Previously Android truncated written dates to milliseconds.
- Reads keep microseconds, including `FieldValue.ServerTimestamp()` values and values written from iOS or a backend. Previously Android truncated them to milliseconds, so a value read back was earlier than the stored one.
- Query filter values and cursor values (`StartingAt`, `StartingAfter`, `EndingAt`, `EndingBefore`) are no longer truncated to milliseconds. A value read from a document now matches that document in `WhereEqualsTo`, and `StartingAfter` with the last value of a page no longer returns that document again. Equality filters against data that older Android builds wrote truncated to milliseconds can stop matching when the filter value has a sub-millisecond part; truncate the filter value to milliseconds while such data remains.
- `RunTransactionAsync<DateTime>` returns the `DateTime` on Android instead of throwing `InvalidCastException`, and date results of a transaction keep their full precision.

On iOS, `DateTime` and `DateTimeOffset` values now also convert directly to and from the native Firestore `Timestamp`, instead of going through `NSDate`. The `NSDate` conversion used a calendar that switches to Julian dates before 1582-10-15, while .NET and Firestore use the proleptic Gregorian calendar. It also went through a double, which cost precision.

- Dates before 1582-10-15 are stored at their exact instant. Previously iOS stored them up to ten days away from it: for example, 0001-03-01 two days early, 1000-01-01 five days late and 1582-10-04 ten days late.
- Dates from 1582-10-15 on are stored at the same instant as before, to within a microsecond. Older iOS builds stored about half of the values with whole milliseconds or microseconds 1 µs early, and a few values with sub-microsecond ticks 1 µs late. Values with whole seconds were stored exactly. Those builds converted query values the same way, so their queries still matched. Now `WhereEqualsTo`, and inclusive range bounds built from the original values, can miss rows that older iOS builds wrote. Query a range that starts 1 µs earlier, and also ends 1 µs later if the values had sub-microsecond ticks, or rewrite those rows.
- Reads of such dates return the stored instant. Previously iOS shifted them the other way: the smallest timestamp read as 0001-01-03, and some instants, such as 1500-03-10, threw `ArgumentOutOfRangeException`. Values that older iOS builds wrote before 1582-10-15 were stored shifted, so they now read back at the shifted instant they were stored at, which Android and other clients already read. Rewrite them if you need the original values.
- `DateTime.MinValue` with `DateTimeKind.Utc`, and `DateTimeOffset.MinValue`, can now be written on iOS. Previously the write failed with an "Invalid timestamp" exception from the native SDK.
- Reads return exactly the stored microsecond. Previously the `NSDate` conversion could move a value into the previous microsecond, so a value read from a document didn't always match that document in `WhereEqualsTo` or work as a `StartingAfter` cursor.
- On iOS, a `DateTime` returned from the `RunTransactionAsync<DateTime>` update function now comes back as the same instant, to the tick, instead of throwing `InvalidCastException`. Its `Kind` is `Utc` even if the returned value was `Local`. A `DateTimeOffset` result also comes back as the same instant to the tick, with a zero offset as before.
- Public iOS helpers in `Plugin.Firebase.Firestore.Platforms.iOS.Extensions`:
  - `NSObjectExtensions.ToNSObject` now returns a `Firebase.Core.Timestamp` for `DateTime` and `DateTimeOffset` values instead of an `NSDate`.
  - `DateExtensions.ToDateTimeOffset(Timestamp)` and `DateExtensions.ToDateTime(Timestamp)` return the exact instant described above, instead of the calendar-shifted value.
  - `NSObjectExtensions.ToObject` converts an `NSDate` from its seconds since the Unix epoch. An `NSDate` before `DateTimeOffset.MinValue` now throws `ArgumentOutOfRangeException`. This includes `NSDate.DistantPast`, which is 0001-01-01 in the Julian calendar and used to convert to `DateTimeOffset.MinValue`. Native Firestore reads return a `Timestamp`, never an `NSDate`, so this only affects an `NSDate` that you pass in yourself.

`null` entries in a stored array now read into typed list properties the same way on both platforms (see [Usage](#usage)). Compared with Plugin.Firebase.Firestore 4.0.0:

- On Android, a list of `string` keeps the `null` entry. Previously it became an empty string. A list of `int`, `long`, `float`, `double`, `decimal` or `bool` throws `ArgumentNullException` when you read `snapshot.Data`. Previously the entry became `0` or `false`.
- On iOS, `null` entries were dropped, so the list was shorter than the stored array and later entries moved to lower indexes: `["a", null, "b"]` read into an `IList<string>` gave two entries. They're now kept, so that list has three entries. A list of a non-nullable value type such as `IList<int>` throws `ArgumentNullException` when you read `snapshot.Data`. Previously `[1, null, 3]` gave `[1, 3]`.
- If a stored array can contain `null`, use a nullable element type such as `IList<int?>` for a list of a value type.

### Firestore exception migration

Firestore methods and snapshot listeners expose native Firebase failures as `CrossPlatformFirebaseFirestoreException` on both platforms. See [Error handling](#error-handling) for the exception and its native metadata.

- Update catches for native exceptions such as `Firebase.Firestore.FirebaseFirestoreException` or `Firebase.FirebaseException` on Android and `Foundation.NSErrorException` on iOS. Catch `CrossPlatformFirebaseFirestoreException` and read `InnerException` if you need the original native exception. Existing catches for `Plugin.Firebase.Core.Exceptions.FirebaseException` still match, including for the iOS methods `GetDocumentSnapshotAsync`, `GetDocumentsAsync`, and `AddDocumentAsync`, which previously reported only the localized description.
- `ITransaction.GetDocument` throws `CrossPlatformFirebaseFirestoreException` instead of `FirebaseFirestoreException` (Android) or a `FirebaseException` without the native error (iOS).
- `RunTransactionAsync` throws `CrossPlatformFirebaseFirestoreException` when the native SDK fails the transaction, for example when the backend rejects the commit, or when the update function lets a failed `ITransaction.GetDocument` propagate. Previously this was `FirebaseFirestoreException` on Android, and `NSErrorException` or a `FirebaseException` wrapping the read failure on iOS.
- On Android, `RunTransactionAsync` throws `FirebaseException` with the original exception as `InnerException` when the update function throws, as on iOS. Previously it threw the native task's `Java.Lang.RuntimeException`.
- The `onError` callback of `AddSnapshotListener` receives `CrossPlatformFirebaseFirestoreException`. Previously it received a `FirebaseException` built from the localized description on iOS, a `FirebaseException` with the native exception as `InnerException` for Android document listeners, and the native `Firebase.FirebaseException` for Android query listeners.
- Some methods now report an exception that the native SDK throws synchronously through the returned `Task`, instead of throwing it before the `Task` is returned. Examples are an `IllegalStateException` on Android after `TerminateAsync`, or an Objective-C exception on iOS. This applies to `IWriteBatch.CommitAsync`, `WaitForPendingWritesAsync`, `DisableNetworkAsync`, `EnableNetworkAsync`, `ClearPersistenceAsync` and `TerminateAsync` on both platforms, and to `IDocumentReference.SetDataAsync`, `UpdateDataAsync` and `DeleteDocumentAsync` on iOS. These exceptions aren't native Firebase failures, so their type doesn't change. Code that awaits the `Task` sees no difference; code that expected the exception before the `Task` was returned has to await it.

On iOS, `RunTransactionAsync` now also retries the transaction after a retryable Firestore error raised inside the update function, when the update function lets it propagate. An example is `ABORTED` from an `ITransaction.GetDocument` that finds the document changed since an earlier read in the same transaction. iOS already re-ran the update function after a retryable commit failure, such as contention with another write; now the native SDK's retry rules decide for errors raised inside the update function too, as they already do on Android. Previously iOS never retried the transaction after such an error. Make sure the update function doesn't change state outside the transaction.

### Unspecified date migration

Writing a `DateTime` whose `Kind` is `DateTimeKind.Unspecified` now throws `ArgumentException` on both platforms. Such a value could be UTC or local time, and the platforms disagreed: Android stored it as local time and iOS stored it as UTC, so the same value became a different instant on each platform unless the device ran in UTC. `Utc` and `Local` values are still stored as their UTC instant, and `DateTimeOffset` values are unaffected.

- The check applies wherever Firestore converts a value: `SetDataAsync` with or without `SetOptions`, `UpdateDataAsync`, batch and transaction writes, `IFirestoreObject` properties, list elements, `FieldValue.ArrayUnion` and `FieldValue.ArrayRemove` elements, query filter values and cursor values.
- In a transaction, `ITransaction.SetData` and `UpdateData` throw inside the update function, so `RunTransactionAsync` fails as it does for any exception thrown there: nothing is committed, and the `ArgumentException` is the `InnerException` of the exception it throws rather than the exception itself.
- `default(DateTime)` and `DateTime.MinValue` are Unspecified, so a `DateTime` property that was never set now throws. Set it, or make the property `DateTime?` or `DateTimeOffset?` if "not set" should be stored and read back as null.
- Typed reads into `DateTime?`, in models and in typed dictionaries such as `Dictionary<string, DateTime?>`, now return a UTC `DateTime`. Previously they threw, because a `DateTimeOffset` came back.
- Common sources of Unspecified values:
  - `new DateTime(...)` without a kind, and `DateTime.Parse` of a string without an offset.
  - `DateTimeOffset.DateTime`, which drops the offset. Use `DateTimeOffset.UtcDateTime` instead.
  - A model's `DateTime` property whose field is missing or null in the document. It reads as `default(DateTime)`, so writing the model back throws.
  - UI controls: .NET MAUI `DatePicker.Date` can be Unspecified.
- `DateTime` values read from Firestore are `Utc`, so writing them back is unaffected.

Say which time zone the value is in, or use `DateTimeOffset`. `DateTimeKind.Utc` keeps the instant iOS stored before, and `DateTimeKind.Local` keeps the one Android stored:

```c#
var dueDate = DateTime.Parse("2026-01-01T09:00:00"); // Kind is Unspecified

// Before: stored as 09:00 UTC on iOS and 09:00 device time on Android. Now: throws ArgumentException.
await document.SetDataAsync(("due", dueDate));

// Either of these works on both platforms.
await document.SetDataAsync(("due", DateTime.SpecifyKind(dueDate, DateTimeKind.Local)));
await document.SetDataAsync(("due", new DateTimeOffset(dueDate, TimeSpan.FromHours(-5))));
```

### Transaction result migration

`RunTransactionAsync` returns the update function’s result directly on both platforms. Earlier releases converted it through the native SDK.

- The result is the same instance that the update function returned. It's no longer deep-copied or normalized through `NSNumber`, `java.lang.Integer` and the other native types. For example, `RunTransactionAsync<object>(_ => 5)` used to return the number as read back from the native value, `RunTransactionAsync<object>(_ => DateTime.UtcNow)` returned a `DateTimeOffset`, and an `IFirestoreObject` came back as a new instance without its `[FirestoreDocumentId]` value.
- Results that the conversion couldn't handle now work. A `DateTime` or array `TResult` used to throw after the writes had been committed. Results such as records, tuples or `HashSet<T>` made the conversion fail inside the update function, which aborted the transaction on Android; on iOS the writes were committed and `RunTransactionAsync` threw afterwards.
- A lazily evaluated result, such as a LINQ query, is returned as-is too, so it's enumerated only after the transaction has finished. Materialize it inside the update function, for example with `ToList()`, if it reads from the transaction.
- An `async` update function, or one that returns a task through an `object` result, now makes `RunTransactionAsync` throw `ArgumentException` without committing (see [Error handling](#error-handling)). Previously the conversion failed on the task inside the update function. On Android that aborted the transaction, and `RunTransactionAsync` threw the native task's `Java.Lang.RuntimeException`. On iOS the failure didn't abort the transaction, so the writes made before the first `await` were committed, and `RunTransactionAsync` then threw a `FirebaseException` carrying the conversion error's message.
- On Android, `Plugin.Firebase.Firestore.Platforms.Android.TransactionFunction<TResult>` is now internal. It was an implementation detail of `RunTransactionAsync`; call `RunTransactionAsync` instead.

### Numeric write migration

Numeric writes follow one set of rules on both platforms (see [Writing numbers](#writing-numbers)):

- `byte`, `sbyte`, `ushort`, `uint` and `ulong` values, and `short` values on Android, can now be written. Previously they were rejected on at least one platform or write path.
- `decimal` and `char` writes throw `ArgumentException` with a message that says how to convert the value. Previously the failure depended on the platform and write path; on Android a `char` model property or dictionary value failed inside the native SDK with a Java `IllegalArgumentException`.
- `ulong` values and `ulong`-backed enums above `long.MaxValue` throw `ArgumentException` instead of wrapping or failing inside the native SDK.
- The public `ToJavaObject` (Android) and `ToNSObject` (iOS) extension methods return a `Java.Lang.Long` or `Java.Lang.Double`, or an int64 or double `NSNumber`, for every number. Previously they returned a value of the source width, such as a `Java.Lang.Integer`.
- On Android, enums in update data, lists, array unions and query operands were converted with wrapping casts: a `uint`-backed enum above `int.MaxValue` became a negative number, and `byte`-, `sbyte`-, `short`- and `ushort`-backed enums failed. They now keep their actual value.
- On iOS, `byte`- and `sbyte`-backed enums were stored as booleans. They are now stored as integers.

Because of the last two changes, anything that compares those enum values with documents that earlier versions wrote with the wrapped or boolean value stops matching them: `WhereEqualsTo`, `WhereFieldIn`, `WhereArrayContains` and `WhereArrayContainsAny` filters, range filters such as `WhereGreaterThan`, and cursors such as `StartingAt`. `FieldValue.ArrayRemove` won't remove the old elements, and `OrderBy` places the old values elsewhere in the order, because wrapped values are negative and booleans sort before all numbers. Rewrite the affected documents if you query, order or remove on those fields.

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
