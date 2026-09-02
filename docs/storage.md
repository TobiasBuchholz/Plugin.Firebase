# Storage

Firebase Storage lets you upload and share user generated content, such as images and video, which allows you to build rich media content into your apps. Firebase Storage stores this data in a Google Cloud Storage bucket, an exabyte scale object storage solution with high availability and global redundancy. Firebase Storage lets you securely upload these files directly from mobile devices and web browsers, handling spotty networks with ease.

## Installation

### NuGet

[![NuGet](https://img.shields.io/nuget/v/Plugin.Firebase.Storage.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Storage/)

> Install-Package Plugin.Firebase.Storage

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/README.md#basic-setup)
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

The .NET 10 release enables nullable-reference analysis for Storage and aligns the shared contracts with values that the native SDKs can omit. These signature changes require callers and custom implementations of the Storage interfaces to account for the following values:

- `IStorageReference.Parent` is nullable and is `null` for the root reference.
- `IStorageListResult.PageToken` is nullable and is `null` for the terminal page.
- `IStorageTaskSnapshot.Metadata` and `Error` are nullable. Successful snapshots have no error, and failure snapshots may have no metadata.
- `IStorageMetadata.Bucket`, `Name`, `Path`, `CacheControl`, `ContentDisposition`, `ContentEncoding`, `ContentLanguage`, `ContentType`, `CustomMetadata`, `MD5Hash`, and `StorageReference` are nullable.
- `IStorageMetadata.Generation`, `MetaGeneration`, `CreationTime`, and `UpdatedTime` are nullable value types. Check `HasValue` before using them.
- Omitted nullable `StorageMetadata` constructor arguments now remain `null` instead of using sentinel `0` or `default(DateTimeOffset)` values. `Size` remains a non-nullable `long` whose default is `0`.

The current iOS Firebase SDK does not expose an object reference through native metadata, so `IStorageMetadata.StorageReference` is always `null` on iOS. Metadata conversion now maps `creationTime` to `CreationTime` and `updatedTime` to `UpdatedTime`, and sends `CacheControl` to the iOS SDK. Code that compensated for the previously swapped timestamps should remove that workaround.

Additional behavioral changes in this release are:

- `ListAllAsync()` on iOS follows native page tokens until it has buffered every item and prefix; its composed result has a terminal `PageToken` of `null`.
- `GetBytesAsync(maxDownloadSizeBytes)` and `GetStreamAsync(maxSize)` on iOS enforce the requested maximum even if the native callback returns a larger payload, and fail with a `FirebaseException` instead of returning the oversized data.
- `IFirebaseStorage.UseEmulator(host, port)` connects the current native Storage instance to the Firebase Storage emulator. Call it before performing any Storage operation.
- iOS task snapshots propagate native snapshot failures through the nullable `IStorageTaskSnapshot.Error` property as `NSErrorException`; inspect that exception when native error details are needed.

## Release notes

- Next
  - Target .NET 10 and raise the minimum Firebase iOS binding version to 12.7; the minimum platform versions remain iOS 15 and Android 23.
  - Corrected nullable reference and value contracts, constructor defaults, timestamp mapping, iOS metadata forwarding, and native task-snapshot error propagation. See [Next-release migration notes](#next-release-migration-notes).
  - Added Storage emulator support, fixed iOS `ListAllAsync()` paging, and enforced iOS in-memory download size limits.
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
