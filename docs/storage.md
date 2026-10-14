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

### Uploading files

`PutFile(filePath, metadata)` hands the path to the native file upload API on both platforms (`putFile(Uri, ...)` on Android, `putFile(_:metadata:completion:)` on iOS), which reads the file from disk instead of buffering it in memory. Read-only files can be uploaded. The platforms differ in these ways:

- **File lifetime:** iOS reads the file after `PutFile` returns and uses the original file as the upload body. Keep the file in place and unchanged until `AwaitAsync()` completes: don't delete, move, or overwrite it, and keep any security-scoped access to it open. Android opens the file when `PutFile` is called. Keeping the file until the upload completes is safe on both platforms.
- **Content type:** when `metadata` is null or has no `ContentType`, iOS infers the content type from the file extension (`photo.jpg` is stored as `image/jpeg`), while Android stores `application/octet-stream`. Set `ContentType` to get the same result on both platforms.
- **Errors:** a missing file or a directory is not thrown from `PutFile`; `AwaitAsync()` fails with the native Storage error on both platforms. On iOS, a null or empty `filePath` throws `ArgumentException` from `PutFile`.
- **Symbolic links:** iOS only uploads regular files, so a path that is a symbolic link fails with a native "not reachable" error when awaited. Pass the link's target path instead.
- **iOS simulator:** the native iOS SDK uploads files through a background URL session. On the simulator this can fail with errors such as `Unexpected 40 code from backend` or `Unexpected -1 code from backend` (the latter when the simulator's background transfer service is unavailable). Firebase iOS SDK 12.17.0 switches simulator file uploads to a foreground session; `AdamE.Firebase.iOS.Storage` bindings older than 12.17.0, including the 12.7.0 minimum this package references, predate that fix. Physical devices are not affected, and `PutBytes` and `PutStream` don't use a background session.

## Next-release migration notes

The .NET 10 release enables nullable-reference analysis for Storage and aligns the shared contracts with values that the native SDKs can omit:

- `IStorageReference.Parent` is null for the root reference.
- `IStorageListResult.PageToken` is null when there is no next page.
- `IStorageTaskSnapshot.Metadata` and `Error` are nullable. Successful snapshots have no error; failure snapshots may have no metadata.
- Optional metadata strings, custom metadata, and `StorageReference` are nullable. The current iOS Firebase SDK no longer exposes a storage reference from metadata, so `StorageReference` is null on iOS.
- `Generation`, `MetaGeneration`, `CreationTime`, and `UpdatedTime` are nullable value types. Check `HasValue` before using them.
- Omitted `StorageMetadata` constructor values now remain null instead of becoming `0` or `default(DateTimeOffset)`.

The release also corrects `StorageMetadata` so `creationTime` and `updatedTime` populate their matching properties, and forwards `CacheControl` when metadata is sent to the iOS SDK. These are breaking behavior and signature changes; update callers and any custom implementations of the Storage interfaces before upgrading.

On iOS, `PutFile` now uploads through the native file upload API instead of reading the whole file into memory and uploading it as data. Large files are no longer copied into memory, and read-only files, which previously failed to open, can now be uploaded. This changes iOS behavior in four ways: the file must stay in place and unchanged until `AwaitAsync()` completes; when no `ContentType` is set, it is inferred from the file extension instead of being stored as `application/octet-stream`; a missing file or a directory now fails the task returned by `PutFile` instead of throwing from `PutFile` (a null or empty path still throws `ArgumentException`); and symbolic links, which were previously followed, now fail. See [Uploading files](#uploading-files) for the remaining platform differences, including a simulator caveat.

On Android, `GetStreamAsync(maxSize)` now returns a readable stream and enforces `maxSize`. The object is downloaded into memory, as on iOS, and the task fails when it is larger than `maxSize`. Previously the returned stream was already closed and `maxSize` was ignored. The public Android-only `StreamProcessor` class that caused this is removed; it has no replacement.

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
