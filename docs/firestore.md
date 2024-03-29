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

To be able to fetch your data from the firestore, you'll need to create a POCO that implements the `IFirestoreObject` interface. Use the `FirestoreDocumentId` and `FirestoreProperty` attributes to hook up its properties, for example:
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

### Further information

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/CloudFirestore/GettingStarted.md) for the Xamarin.Firebase.iOS.CloudFirestore packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseFirestore.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IFirebaseFirestore.cs)
- [src/.../ICollectionReference.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/ICollectionReference.cs)
- [src/.../IDocumentReference.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IDocumentReference.cs)
- [src/.../IQuery.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IQuery.cs)
- [src/.../ITransaction.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/ITransaction.cs)
- [src/.../IWriteBatch.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IWriteBatch.cs)
- [tests/.../FirestoreFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/Firestore/FirestoreFixture.cs)

## Release notes
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
