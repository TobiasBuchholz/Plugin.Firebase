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

## Uploading files

`PutFile(filePath, metadata)` hands the path to the native file upload API on both platforms (`putFile(Uri, ...)` on Android, `putFile(_:metadata:completion:)` on iOS), which reads the file from disk instead of buffering it in memory. Read-only files can be uploaded. The platforms differ in these ways:

- **File lifetime:** iOS reads the file after `PutFile` returns and uses the original file as the upload body. Keep the file in place and unchanged until `AwaitAsync()` completes: don't delete, move, or overwrite it, and keep any security-scoped access to it open. Android opens the file when `PutFile` is called. Keeping the file until the upload completes is safe on both platforms.
- **Content type:** when `metadata` is null or has no `ContentType`, iOS infers the content type from the file extension (`photo.jpg` is stored as `image/jpeg`), while Android stores `application/octet-stream`. Set `ContentType` to get the same result on both platforms.
- **Errors:** a missing file or a directory is not thrown from `PutFile`; `AwaitAsync()` fails with a `CrossPlatformFirebaseStorageException` that preserves the native error on both platforms. A null or empty `filePath` throws `ArgumentException` from `PutFile` on both platforms.
- **Symbolic links:** iOS only uploads regular files, so a path that is a symbolic link fails with a native "not reachable" error when awaited. Pass the link's target path instead.
- **Background session:** on physical iOS devices, the SDK uses a background URL session and the system may defer uploads started while the app is in the background. The iOS simulator uses a foreground session with the current binding.

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

The .NET 10 release enables nullable-reference analysis for Storage and aligns the shared contracts with values that the native SDKs can omit:

- `IStorageReference.Parent` is null for the root reference.
- `IStorageListResult.PageToken` is null when there is no next page.
- `IStorageTaskSnapshot.Metadata` and `Error` are nullable. Successful snapshots have no error; failure snapshots may have no metadata.
- Optional metadata strings, custom metadata, and `StorageReference` are nullable. The current iOS Firebase SDK no longer exposes a storage reference from metadata, so `StorageReference` is null on iOS.
- `Generation`, `MetaGeneration`, `CreationTime`, and `UpdatedTime` are nullable value types. Check `HasValue` before using them.
- Omitted `StorageMetadata` constructor values now remain null instead of becoming `0` or `default(DateTimeOffset)`.

The release also corrects `StorageMetadata` so `creationTime` and `updatedTime` populate their matching properties, and forwards `CacheControl` when metadata is sent to the iOS SDK. These are breaking behavior and signature changes; update callers and any custom implementations of the Storage interfaces before upgrading.

On iOS, `PutFile` now uploads through the native file upload API instead of reading the whole file into memory and uploading it as data. Large files are no longer copied into memory, and read-only files, which previously failed to open, can now be uploaded. This changes iOS behavior in four ways: the file must stay in place and unchanged until `AwaitAsync()` completes; when no `ContentType` is set, it is inferred from the file extension instead of being stored as `application/octet-stream`; a missing file or a directory now fails the task returned by `PutFile` instead of throwing from `PutFile` (a null or empty path still throws `ArgumentException`); and symbolic links, which were previously followed, now fail. On physical devices, file uploads now run in a background URL session outside the app process, so the system may defer an upload that starts while the app is in the background; the previous data upload ran in the app's own session. See [Uploading files](#uploading-files) for the remaining platform differences, including background behavior.

On Android, a null or empty `filePath` passed to `PutFile` now throws `ArgumentException`, matching iOS. Previously it was passed to the native SDK unchecked.

On Android, `GetStreamAsync(maxSize)` now returns a readable stream and enforces `maxSize`. The object is downloaded into memory, as on iOS, and the task fails when it is larger than `maxSize`. Previously the returned stream was already closed and `maxSize` was ignored. The public Android-only `StreamProcessor` class that caused this is removed; it has no replacement.

Failures that the native Storage SDKs report now surface as `CrossPlatformFirebaseStorageException` on both platforms (see [Error handling](#error-handling)). These are breaking changes; update `catch` blocks and failure observers before upgrading:

- On Android, tasks returned by `IStorageReference` methods and `IStorageTransferTask.AwaitAsync()` previously faulted with the native `Firebase.Storage.StorageException`. A `catch` for `Firebase.Storage.StorageException`, or for its native base types `Firebase.FirebaseException` and `Java.Lang.Exception`, no longer matches; catch `CrossPlatformFirebaseStorageException` and read `NativeErrorCode` and `NativeHttpResultCode`, or the native exception from `InnerException`. A `catch (FirebaseException)` for `Plugin.Firebase.Core.Exceptions.FirebaseException` didn't match these failures before and now does.
- On iOS, `GetMetadataAsync`, `UpdateMetadataAsync`, `GetDownloadUrlAsync` and `DeleteAsync` previously faulted with `Foundation.NSErrorException`; `ListAsync`, `ListAllAsync`, `GetStreamAsync` and `GetBytesAsync` with a `FirebaseException` that carried only the localized description; and the transfer tasks returned by `PutBytes`, `PutFile`, `PutStream` and `DownloadFile` with a plain `System.Exception`. A `catch` for `Foundation.NSErrorException` no longer matches; the `NSErrorException` is now the `InnerException`. `CrossPlatformFirebaseStorageException` derives from `FirebaseException`, so an existing `catch (FirebaseException)` still matches, and now also catches the failures that were `NSErrorException` or plain `Exception`.
- `IStorageTaskSnapshot.Error` on failure snapshots is a `CrossPlatformFirebaseStorageException`. Previously it was the native `StorageException` on Android and `NSErrorException` on iOS, so a type check against `Firebase.Storage.StorageException`, `Firebase.FirebaseException`, `Java.Lang.Exception` or `Foundation.NSErrorException` no longer matches.
- On iOS, when the native SDK returns more data than the requested maximum size, `GetStreamAsync` and `GetBytesAsync` fail with `CrossPlatformFirebaseStorageException` (`-13032`, the native `downloadSizeExceeded` error) instead of a plain `FirebaseException`.
- On Android, `ListAsync(maxResults)` throws `ArgumentOutOfRangeException` synchronously if the value is outside the 32-bit integer range. Previously the cast could truncate the value, so `4294967297` listed one item. The native SDKs enforce their own page-size limits.

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
