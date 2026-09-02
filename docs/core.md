# Core

Plugin.Firebase.Core provides the shared Firebase initialization layer used by the feature packages. It is installed transitively with those packages and with the bundled `Plugin.Firebase` package, so most applications do not need to reference it directly.

## Installation

### NuGet

[![NuGet](https://img.shields.io/nuget/v/Plugin.Firebase.Core.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.Core/)

> Install-Package Plugin.Firebase.Core

## Setup

Follow the repository's [basic setup instructions](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/development/README.md#basic-setup). Core supplies the platform-specific `CrossFirebase.Initialize(...)` entry points and coordinates initialization hooks used by the feature packages.

The package supports .NET 10 only: `net10.0`, `net10.0-android`, and `net10.0-ios`. MAUI consumers must use .NET MAUI 10; compatible non-MAUI .NET 10 mobile apps can also use the package. The minimum operating-system versions are iOS 15 and Android API level 23.

## Release notes

- Next
  - Target .NET 10 only and require .NET MAUI 10 for MAUI consumers.
  - Raise the minimum Firebase iOS binding version to 12.7 while retaining iOS 15 and Android API level 23 as the minimum operating-system versions.
  - Make the `TryGetDefaultApp` out parameter nullable when no default Android Firebase app exists.
