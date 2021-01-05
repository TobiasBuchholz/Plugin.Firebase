# Storage

Firebase Storage lets you upload and share user generated content, such as images and video, which allows you to build rich media content into your apps. Firebase Storage stores this data in a Google Cloud Storage bucket, an exabyte scale object storage solution with high availability and global redundancy. Firebase Storage lets you securely upload these files directly from mobile devices and web browsers, handling spotty networks with ease.

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Storage at your project in the [Firebase Console](https://console.firebase.google.com/)
- Initialize CrossFirebase with Storage enabled:

```
  CrossFirebase.Initialize(..., new CrossFirebaseSettings(isStorageEnabled:true));
```

## Usage

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/Storage/GettingStarted.md) for the Xamarin.Firebase.iOS.Storage packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseStorage.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Storage/IFirebaseStorage.cs)
- [tests/.../StorageFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/Firestore/StorageFixture.cs)
