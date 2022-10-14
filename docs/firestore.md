# Firestore

Cloud Firestore is a flexible, scalable database for mobile, web, and server development from Firebase and Google Cloud. Like Firebase Realtime Database, it keeps your data in sync across client apps through realtime listeners and offers offline support for mobile and web so you can build responsive apps that work regardless of network latency or Internet connectivity. Cloud Firestore also offers seamless integration with other Firebase and Google Cloud products, including Cloud Functions.

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Cloud Firestore at your project in the [Firebase Console](https://console.firebase.google.com/)
- Initialize CrossFirebase with Firestore enabled:

```c#
  CrossFirebase.Initialize(..., new CrossFirebaseSettings(isFirestoreEnabled:true));
```

## Usage

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/CloudFirestore/GettingStarted.md) for the Xamarin.Firebase.iOS.CloudFirestore packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseFirestore.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IFirebaseFirestore.cs)
- [src/.../ICollectionReference.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/ICollectionReference.cs)
- [src/.../IDocumentReference.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IDocumentReference.cs)
- [src/.../IQuery.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IQuery.cs)
- [src/.../ITransaction.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/ITransaction.cs)
- [src/.../IWriteBatch.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Firestore/IWriteBatch.cs)
- [tests/.../FirestoreFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/Firestore/FirestoreFixture.cs)

### Receiving Data From Firestore

To exchange data with Firestore, attributes need to be added to public properties in data classes. Without these attributes, the returned data object from calls to Firestore will contain all null fields.

For example:
```csharp
[FirestoreDocumentId]
public string FireStoreId
{
    get => Id;
    set => Id = value;
}

[FirestoreProperty(nameof(Name))]
public string FireStoreName { 
    get => Name;
    set => Name = value;
}
```
The `nameof(Name)` lets the plugin know that it expects a Firebase field named "Name" when synchronizing, when using `IDocumentSnapshot.Data` etc.


### Compatibility with CommunityToolkit.Mvvm.ComponentModel

If you use `[ObservableProperty]` From the CommunityToolkit.Mvvm.ComponentModel package, This can cause problems since this attribute needs to be on fields (properties are auto-generated in a separate file), whereas `[FirestoreProperty]` needs to be on public properties.

A simple solution is to make the `[FirestoreProperty]` property a simple wrapper around the autogenetrated property. 

```csharp
[ObservableProperty]
string _name;

[FirestoreProperty(nameof(Name))]
public string FireStoreName { 
    get => Name;
    set => Name = value;
}
```
This means that any time firestore updates the public property, the notification events still fire and are bindable from your UI in the normal fashion.