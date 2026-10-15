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

## Error handling

Every failure that the native Storage SDK reports as a `com.google.firebase.FirebaseException` (Android, which includes `StorageException`) or an `NSError` (iOS) surfaces as `CrossPlatformFirebaseStorageException` (`Plugin.Firebase.Core.Exceptions`). That covers the tasks returned by `IStorageReference`, `IStorageTransferTask.AwaitAsync()` for uploads and downloads, and `IStorageTaskSnapshot.Error` on failure snapshots. The type derives from `CrossPlatformFirebaseException`, the base that the Auth and Firestore exceptions share, and from `FirebaseException`. It keeps the native exception as `InnerException` and passes the native metadata through:

| Property | Android | iOS |
| --- | --- | --- |
| `NativeExceptionTypeName` | Java canonical class name, e.g. `com.google.firebase.storage.StorageException` | `NSError` |
| `NativeErrorCode` | `StorageException.ErrorCode`, e.g. `-13010`; `null` for other Firebase exceptions | `NSError` code, e.g. `-13010` (`StorageErrorCode.objectNotFound`) |
| `NativeErrorDomain` | `null` | `NSError` domain, e.g. `FIRStorageErrorDomain` |
| `NativeHttpResultCode` | `StorageException.HttpResultCode`, e.g. `404`; `0` or another non-HTTP value when the failure didn't come from an HTTP response | HTTP status of the failed request, e.g. `404`, read from the `ResponseErrorCode` user-info entry when `ResponseErrorDomain` is `com.google.HTTPStatus`; otherwise `null` |
| `NativeErrorMessage` | The exception message | The `NSError` localized description |

`NativeErrorCode` is the number formatted with the invariant culture, so it reads the same in every locale. The Storage codes match across the platforms where both SDKs define them, for example `-13010` for an object that doesn't exist and `-13020` and `-13021` for unauthenticated and unauthorized requests. Some failures differ by platform:

- When a download exceeds the maximum size passed to `GetStreamAsync` or `GetBytesAsync`, Android reports `-13000` (`ERROR_UNKNOWN`, with the native size failure as the Java cause) and iOS reports `-13032` (`downloadSizeExceeded`).
- iOS defines codes that Android doesn't, such as `-13050` (`invalidArgument`).

Only native Firebase failures are wrapped. Managed exceptions, such as a `FirebaseException` for a missing native result, propagate unchanged, as does argument validation by the native SDKs. `ListAsync(maxResults)` validates `maxResults` itself and throws `ArgumentOutOfRangeException` on both platforms when it isn't between 1 and 1000, the range that both native SDKs accept.

```c#
try {
    var metadata = await CrossFirebaseStorage.Current.GetReferenceFromPath("files/missing.txt").GetMetadataAsync();
} catch(CrossPlatformFirebaseStorageException e) when(e.NativeErrorCode == "-13010") {
    Console.WriteLine($"Not found (HTTP {e.NativeHttpResultCode}): {e.NativeErrorMessage}");
}
```

## Next-release migration notes

The .NET 10 release enables nullable-reference analysis for Storage and aligns the shared contracts with values that the native SDKs can omit:

- `IStorageReference.Parent` is null for the root reference.
- `IStorageListResult.PageToken` is null when there is no next page.
- `IStorageTaskSnapshot.Metadata` and `Error` are nullable. Successful snapshots have no error; failure snapshots may have no metadata.
- Optional metadata strings, custom metadata, and `StorageReference` are nullable. The current iOS Firebase SDK no longer exposes a storage reference from metadata, so `StorageReference` is null on iOS.
- `Generation`, `MetaGeneration`, `CreationTime`, and `UpdatedTime` are nullable value types. Check `HasValue` before using them.
- Omitted `StorageMetadata` constructor values now remain null instead of becoming `0` or `default(DateTimeOffset)`.

The release also corrects `StorageMetadata` so `creationTime` and `updatedTime` populate their matching properties, and forwards `CacheControl` when metadata is sent to the iOS SDK. These are breaking behavior and signature changes; update callers and any custom implementations of the Storage interfaces before upgrading.

On Android, `GetStreamAsync(maxSize)` now returns a readable stream and enforces `maxSize`. The object is downloaded into memory, as on iOS, and the task fails when it is larger than `maxSize`. Previously the returned stream was already closed and `maxSize` was ignored. The public Android-only `StreamProcessor` class that caused this is removed; it has no replacement.

Failures that the native Storage SDKs report now surface as `CrossPlatformFirebaseStorageException` on both platforms (see [Error handling](#error-handling)). These are breaking changes; update `catch` blocks and failure observers before upgrading:

- On Android, tasks returned by `IStorageReference` methods and `IStorageTransferTask.AwaitAsync()` previously faulted with the native `Firebase.Storage.StorageException`. A `catch (StorageException)` no longer matches; catch `CrossPlatformFirebaseStorageException` and read `NativeErrorCode` and `NativeHttpResultCode`, or the native exception from `InnerException`.
- On iOS, `GetMetadataAsync`, `UpdateMetadataAsync`, `GetDownloadUrlAsync` and `DeleteAsync` previously faulted with `Foundation.NSErrorException`; `ListAsync`, `ListAllAsync`, `GetStreamAsync` and `GetBytesAsync` with a `FirebaseException` that carried only the localized description; and the transfer tasks returned by `PutBytes`, `PutFile`, `PutStream` and `DownloadFile` with a plain `System.Exception`. A `catch (NSErrorException)` no longer matches. `CrossPlatformFirebaseStorageException` derives from `FirebaseException`, so an existing `catch (FirebaseException)` still matches, and now also catches the failures that were `NSErrorException` or plain `Exception`.
- `IStorageTaskSnapshot.Error` on failure snapshots is a `CrossPlatformFirebaseStorageException`. Previously it was the native `StorageException` on Android and `NSErrorException` on iOS.
- On iOS, when the native SDK returns more data than the requested maximum size, `GetStreamAsync` and `GetBytesAsync` fail with `CrossPlatformFirebaseStorageException` (`-13032`, the native `downloadSizeExceeded` error) instead of a plain `FirebaseException`.
- `ListAsync(maxResults)` throws `ArgumentOutOfRangeException` before calling the native SDK when `maxResults` is less than 1 or greater than 1000. Previously Android threw `Java.Lang.IllegalArgumentException` for 0 or values above 1000 and silently truncated values outside the `int` range (for example, `4294967297` listed one item), and iOS faulted with a `FirebaseException` built from the native invalid-argument error.

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
