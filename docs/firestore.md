# Firestore

Cloud Firestore is a flexible, scalable database for mobile, web, and server development from Firebase and Google Cloud. Like Firebase Realtime Database, it keeps your data in sync across client apps through realtime listeners and offers offline support for mobile and web so you can build responsive apps that work regardless of network latency or Internet connectivity. Cloud Firestore also offers seamless integration with other Firebase and Google Cloud products, including Cloud Functions.

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Cloud Firestore at your project in the [Firebase Console](https://console.firebase.google.com/)
- Initialize CrossFirebase with Firestore enabled:

```
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
