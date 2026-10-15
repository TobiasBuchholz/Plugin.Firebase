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
`InnerException` is the original exception, on both platforms. A `CrossPlatformFirebaseFirestoreException` that
the update function lets propagate, for example from a failed `ITransaction.GetDocument`, isn't wrapped again:
`RunTransactionAsync` throws a `CrossPlatformFirebaseFirestoreException` for the same native error. On Android the
native transaction runner first applies its retry rules to that error; on iOS the transaction isn't retried. On
Android the same applies to a native `FirebaseFirestoreException` that the update function throws directly.

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

Failures that the native Firestore SDKs report now surface as `CrossPlatformFirebaseFirestoreException` on both platforms (see [Error handling](#error-handling)). These are breaking changes; update `catch` blocks and `onError` handlers before upgrading:

- Tasks returned by Firestore methods previously faulted with the native exception: `Firebase.Firestore.FirebaseFirestoreException` (or another `Firebase.FirebaseException`) on Android, and `Foundation.NSErrorException` on iOS. On iOS, `GetDocumentSnapshotAsync`, `GetDocumentsAsync` and `AddDocumentAsync` faulted with a `FirebaseException` that carried only the localized description. A `catch` for any of these no longer matches; catch `CrossPlatformFirebaseFirestoreException` and read the native exception from `InnerException` if you need it.
- `ITransaction.GetDocument` throws `CrossPlatformFirebaseFirestoreException` instead of `FirebaseFirestoreException` (Android) or a `FirebaseException` without the native error (iOS).
- `RunTransactionAsync` throws `CrossPlatformFirebaseFirestoreException` when the native SDK fails the transaction, for example when the backend rejects the commit, or when the update function lets a failed `ITransaction.GetDocument` propagate. Previously this was `FirebaseFirestoreException` on Android, and `NSErrorException` or a `FirebaseException` wrapping the read failure on iOS.
- On Android, `RunTransactionAsync` throws `FirebaseException` with the original exception as `InnerException` when the update function throws, as on iOS. Previously it threw the native task's `Java.Lang.RuntimeException`.
- The `onError` callback of `AddSnapshotListener` receives `CrossPlatformFirebaseFirestoreException`. Previously it received a `FirebaseException` built from the localized description on iOS, a `FirebaseException` with the native exception as `InnerException` for Android document listeners, and the native `Firebase.FirebaseException` for Android query listeners.
- Some methods now report an exception that the native SDK throws synchronously through the returned `Task`, instead of throwing it before the `Task` is returned. Examples are an `IllegalStateException` on Android after `TerminateAsync`, or an Objective-C exception on iOS. This applies to `IWriteBatch.CommitAsync`, `WaitForPendingWritesAsync`, `DisableNetworkAsync`, `EnableNetworkAsync`, `ClearPersistenceAsync` and `TerminateAsync` on both platforms, and to `IDocumentReference.SetDataAsync`, `UpdateDataAsync` and `DeleteDocumentAsync` on iOS. These exceptions aren't native Firebase failures, so their type doesn't change. Code that awaits the `Task` sees no difference; code that expected the exception before the `Task` was returned has to await it.

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
