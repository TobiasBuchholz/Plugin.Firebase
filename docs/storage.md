# Storage

Firebase Storage lets you upload and share user generated content, such as images and video, which allows you to build rich media content into your apps. Firebase Storage stores this data in a Google Cloud Storage bucket, an exabyte scale object storage solution with high availability and global redundancy. Firebase Storage lets you securely upload these files directly from mobile devices and web browsers, handling spotty networks with ease.

## Installation
### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.storage.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Storage/)

> Install-Package Plugin.Firebase.Storage

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Storage at your project in the [Firebase Console](https://console.firebase.google.com/)

## Usage

Take a look at the [documentation](https://github.com/xamarin/GoogleApisForiOSComponents/blob/master/docs/Firebase/Storage/GettingStarted.md) for the Xamarin.Firebase.iOS.Storage packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseStorage.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Storage/IFirebaseStorage.cs)
- [src/.../IStorageReference.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Storage/IStorageReference.cs)
- [src/.../IStorageMetaData.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Storage/IStorageMetaData.cs)
- [src/.../IStorageTransferTask.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Storage/IStorageTransferTask.cs)
- [src/.../IStorageReference.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Storage/IStorageReference.cs)
- [tests/.../StorageFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/Firestore/StorageFixture.cs)

## Release notes
- Version 2.0.3
  - Add missing GetBytes() method to IStorageReference (issue #279)
- Version 2.0.2
  - Bumped up Xamarin.Firebase.Storage package to version 120.2.1.3
  - Bumped up Xamarin.Firebase.Storage.Common package to version 117.0.0.12
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm
