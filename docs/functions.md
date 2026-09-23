# Functions

Cloud Functions for Firebase is a serverless framework that lets you automatically run backend code in response to events triggered by Firebase features and HTTPS requests. Your JavaScript or TypeScript code is stored in Google's cloud and runs in a managed environment. There's no need to manage and scale your own servers.

## Installation

### NuGet

[![NuGet](https://img.shields.io/nuget/v/Plugin.Firebase.Functions.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Functions/)

> Install-Package Plugin.Firebase.Functions

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/README.md#basic-setup)
- Enable Cloud Functions at your project in the [Firebase Console](https://console.firebase.google.com/)
- [Deploy](https://firebase.google.com/docs/functions/get-started?hl=en) your own function
- Call `CrossFirebaseFunctions.Initialize(string? region)` if your functions are deployed outside the default `us-central1` region

Call `CrossFirebaseFunctions.Initialize(string? region)` before accessing `CrossFirebaseFunctions.Current` or `CrossFirebaseFunctions.IsSupported`. If you change the region after `Current` has already been created, reacquire `CrossFirebaseFunctions.Current` before creating new callable references. Existing `IFirebaseFunctions` and `IHttpsCallable` references keep using the instance they were created from.

## Next-release migration notes

Typed callable responses now convert the complete native payload instead of deserializing the native object's display string. Object, array, scalar, and null responses can therefore be requested as DTOs or `JsonElement` values. When `TResponse` is `string`, native string payloads remain unquoted strings, while object and array payloads are returned as valid JSON. A native null payload returns `default(TResponse)`.

Regional initialization now recreates an already-created `CrossFirebaseFunctions.Current` instance when the configured region changes and reapplies its emulator host and port. Reacquire `Current` and create new callable references after calling `Initialize`; previously acquired `IFirebaseFunctions` and `IHttpsCallable` references continue to use their original instance.

The region continues to be configured through the global `CrossFirebaseFunctions.Initialize(...)` entry point. The breaking per-region instance redesign proposed in [#661](https://github.com/TobiasBuchholz/Plugin.Firebase/issues/661) is not included in this release.

## Usage

Take a look at the [documentation](https://firebase.google.com/docs/functions/callable?hl=en#call_the_function) for the official Firebase Cloud Function SDKs, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [IFirebaseFunctions.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/src/Functions/Shared/IFirebaseFunctions.cs)
- [IHttpsCallable.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/src/Functions/Shared/IHttpsCallable.cs)
- [Functions integration tests](https://github.com/TobiasBuchholz/Plugin.Firebase/tree/development/tests/Plugin.Firebase.IntegrationTests/Functions)
- [Test Cloud Functions](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/tests/cloud-functions/functions/src/index.ts)

## Release notes

- Next
  - Target .NET 10 and raise the minimum Firebase iOS binding version to 12.7; the minimum platform versions remain iOS 15 and Android 23.
  - Fixed typed callable responses for native object, array, scalar, and null payloads.
  - Fixed regional initialization after `Current` is created and preserved emulator settings when changing regions.
  - Kept global region initialization; the breaking redesign in #661 is explicitly excluded.
- Version 3.1.1
  - Using AdamE.Firebase.iOS.* minimum version 11
- Version 3.1.0
  - Update to .net8
- Version 3.0.0
  - Swapped Xamarin.Firebase.iOS.CloudFunctions (native SDK 8.10.0) for AdamE.Firebase.iOS.CloudFunctions (native SDK 10.24.0)
- Version 2.0.3
  - Added support for non-default regions
- Version 2.0.2
  - Bumped up Xamarin.Firebase.Functions package to version 120.3.1.3
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm
