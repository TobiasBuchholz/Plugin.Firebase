# Storage

Firebase Storage lets you upload and share user generated content, such as images and video, which allows you to build rich media content into your apps. Firebase Storage stores this data in a Google Cloud Storage bucket, an exabyte scale object storage solution with high availability and global redundancy. Firebase Storage lets you securely upload these files directly from mobile devices and web browsers, handling spotty networks with ease.

## Installation

### NuGet

[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.storage.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Storage/)

> Install-Package Plugin.Firebase.Storage

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Storage at your project in the [Firebase Console](https://console.firebase.google.com/)

## Usage

Take a look at the [documentation](https://github.com/AdamEssenmacher/GoogleApisForiOSComponents/blob/master/docs/Firebase/Storage/GettingStarted.md) for the AdamE.Firebase.iOS.Storage packages, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:

- [IFirebaseStorage.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/src/Storage/Shared/IFirebaseStorage.cs)
- [IStorageReference.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/src/Storage/Shared/IStorageReference.cs)
- [IStorageMetaData.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/src/Storage/Shared/IStorageMetaData.cs)
- [IStorageTransferTask.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/src/Storage/Shared/IStorageTransferTask.cs)
- [IStorageTaskSnapshot.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/src/Storage/Shared/IStorageTaskSnapshot.cs)
- [Storage integration tests](https://github.com/TobiasBuchholz/Plugin.Firebase/tree/development/tests/Plugin.Firebase.IntegrationTests/Storage)

## Next-release migration notes

The .NET 10 release enables nullable-reference analysis for Storage and aligns the shared contracts with values that the native SDKs can omit:

- `IStorageReference.Parent` is null for the root reference.
- `IStorageListResult.PageToken` is null when there is no next page.
- `IStorageTaskSnapshot.Metadata` and `Error` are nullable. Successful snapshots have no error; failure snapshots may have no metadata.
- Optional metadata strings, custom metadata, and `StorageReference` are nullable. The current iOS Firebase SDK no longer exposes a storage reference from metadata, so `StorageReference` is null on iOS.
- `Generation`, `MetaGeneration`, `CreationTime`, and `UpdatedTime` are nullable value types. Check `HasValue` before using them.
- Omitted `StorageMetadata` constructor values now remain null instead of becoming `0` or `default(DateTimeOffset)`.

The release also corrects `StorageMetadata` so `creationTime` and `updatedTime` populate their matching properties, and forwards `CacheControl` when metadata is sent to the iOS SDK. These are breaking behavior and signature changes; update callers and any custom implementations of the Storage interfaces before upgrading.

## Release notes

- Version 3.1.1
  - Using AdamE.Firebase.iOS.* minimum version 11
- Version 3.1.0
  - Update to .net8
- Version 3.0.0
  - Swapped Xamarin.Firebase.iOS.Storage (native SDK 8.10.0) for AdamE.Firebase.iOS.Storage (native SDK 10.24.0)
- Version 2.0.3
  - Add missing GetBytes() method to IStorageReference (issue #279)
- Version 2.0.2
  - Bumped up Xamarin.Firebase.Storage package to version 120.2.1.3
  - Bumped up Xamarin.Firebase.Storage.Common package to version 117.0.0.12
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm
