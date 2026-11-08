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

## Error handling

Every failure that the native Storage SDK reports as a `com.google.firebase.FirebaseException` (Android, which includes `StorageException`) or an `NSError` (iOS) surfaces as `CrossPlatformFirebaseStorageException` (`Plugin.Firebase.Core.Exceptions`). That covers the tasks returned by `IStorageReference`, `IStorageTransferTask.AwaitAsync()` for uploads and downloads, and `IStorageTaskSnapshot.Error` on failure snapshots. The type derives from `CrossPlatformFirebaseException`, the base that the Auth and Firestore exceptions share, and from `FirebaseException`. It keeps the native exception as `InnerException` and passes the native metadata through:

| Property | Android | iOS |
| --- | --- | --- |
| `NativeExceptionTypeName` | Java canonical class name, e.g. `com.google.firebase.storage.StorageException` | `NSError` |
| `NativeErrorCode` | `StorageException.ErrorCode`, e.g. `-13010`; `null` for other Firebase exceptions | `NSError` code, e.g. `-13010` (`StorageErrorCode.objectNotFound`) |
| `NativeErrorDomain` | `null` | `NSError` domain, e.g. `FIRStorageErrorDomain` |
| `NativeHttpResultCode` | `StorageException.HttpResultCode`, e.g. `404`; `0` or another non-HTTP value when the failure didn't come from an HTTP response | HTTP status of the failed request, e.g. `404`, read from the `ResponseErrorCode` user-info entry when `ResponseErrorDomain` is `com.google.HTTPStatus`; otherwise `null` |
| `NativeErrorMessage` | The exception message | The `NSError` localized description |

`NativeErrorCode` is the number formatted with the invariant culture, so it reads the same in every locale. The Storage codes match across the platforms where both SDKs define them, for example `-13010` for an object that doesn't exist and `-13020` and `-13021` for unauthenticated and unauthorized requests. The size limit is the exception: when a download exceeds the maximum size passed to `GetStreamAsync` or `GetBytesAsync`, Android reports `-13000` (`ERROR_UNKNOWN`, with the native size failure as the Java cause) and iOS reports `-13032` (`downloadSizeExceeded`), a code that the Android SDK doesn't define.

Only native Firebase failures are wrapped. Managed exceptions, such as a `FirebaseException` for a missing native result, propagate unchanged, as does argument validation by the native SDKs. For `ListAsync(maxResults)`, the native SDK enforces the supported page-size range.

```c#
try {
    var metadata = await CrossFirebaseStorage.Current.GetReferenceFromPath("files/missing.txt").GetMetadataAsync();
} catch(CrossPlatformFirebaseStorageException e) when(e.NativeErrorCode == "-13010") {
    Console.WriteLine($"Not found (HTTP {e.NativeHttpResultCode}): {e.NativeErrorMessage}");
}
```

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

On Android, `GetStreamAsync(maxSize)` now returns a readable stream and enforces `maxSize`. The object is downloaded into memory, as on iOS, and the task fails when it is larger than `maxSize`. Previously the returned stream was already closed and `maxSize` was ignored. The public Android-only `StreamProcessor` class that caused this is removed; it has no replacement.

Failures that the native Storage SDKs report now surface as `CrossPlatformFirebaseStorageException` on both platforms (see [Error handling](#error-handling)). These are breaking changes; update `catch` blocks and failure observers before upgrading:

- On Android, tasks returned by `IStorageReference` methods and `IStorageTransferTask.AwaitAsync()` previously faulted with the native `Firebase.Storage.StorageException`. A `catch` for `Firebase.Storage.StorageException`, or for its native base types `Firebase.FirebaseException` and `Java.Lang.Exception`, no longer matches; catch `CrossPlatformFirebaseStorageException` and read `NativeErrorCode` and `NativeHttpResultCode`, or the native exception from `InnerException`. A `catch (FirebaseException)` for `Plugin.Firebase.Core.Exceptions.FirebaseException` didn't match these failures before and now does.
- On iOS, `GetMetadataAsync`, `UpdateMetadataAsync`, `GetDownloadUrlAsync` and `DeleteAsync` previously faulted with `Foundation.NSErrorException`; `ListAsync`, `ListAllAsync`, `GetStreamAsync` and `GetBytesAsync` with a `FirebaseException` that carried only the localized description; and the transfer tasks returned by `PutBytes`, `PutFile`, `PutStream` and `DownloadFile` with a plain `System.Exception`. A `catch` for `Foundation.NSErrorException` no longer matches; the `NSErrorException` is now the `InnerException`. `CrossPlatformFirebaseStorageException` derives from `FirebaseException`, so an existing `catch (FirebaseException)` still matches, and now also catches the failures that were `NSErrorException` or plain `Exception`.
- `IStorageTaskSnapshot.Error` on failure snapshots is a `CrossPlatformFirebaseStorageException`. Previously it was the native `StorageException` on Android and `NSErrorException` on iOS, so a type check against `Firebase.Storage.StorageException`, `Firebase.FirebaseException`, `Java.Lang.Exception` or `Foundation.NSErrorException` no longer matches.
- On iOS, when the native SDK returns more data than the requested maximum size, `GetStreamAsync` and `GetBytesAsync` fail with `CrossPlatformFirebaseStorageException` (`-13032`, the native `downloadSizeExceeded` error) instead of a plain `FirebaseException`.
- On Android, `ListAsync(maxResults)` throws `ArgumentOutOfRangeException` synchronously if the value is outside the 32-bit integer range. Previously the cast could truncate the value, so `4294967297` listed one item. The native SDKs enforce their own page-size limits.

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
