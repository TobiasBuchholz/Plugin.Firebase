# Functions

Cloud Functions for Firebase is a serverless framework that lets you automatically run backend code in response to events triggered by Firebase features and HTTPS requests. Your JavaScript or TypeScript code is stored in Google's cloud and runs in a managed environment. There's no need to manage and scale your own servers.

## Installation
### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.functions.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Functions/)

> Install-Package Plugin.Firebase.Functions

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Cloud Functions at your project in the [Firebase Console](https://console.firebase.google.com/)
- [Deploy](https://firebase.google.com/docs/functions/get-started?hl=en) your own function
- Call `CrossFirebaseFunctions.Initialize(string region)` if your functions are deployed outside the default `us-central1` region

## Usage

Take a look at the [documentation](https://firebase.google.com/docs/functions/callable?hl=en#call_the_function) for the official Firebase Cloud Function SKDs, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseFunctions.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Functions/IFirebaseFunctions.cs)
- [src/.../IHttpsCallable.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Functions/IHttpsCallable.cs)
- [tests/.../FunctionsFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/Functions/FunctionsFixture.cs)
- [tests/cloud-functions/.../index.ts](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/cloud-functions/functions/src/index.ts)

## Release notes
- Version 2.0.3
  - Added support for non-default regions
- Version 2.0.2
  - Bumped up Xamarin.Firebase.Functions package to version 120.3.1.3
- Version 2.0.1
  - Remove unnecessary UseMaui property from csproj files
  - Readd net6.0 tfm
