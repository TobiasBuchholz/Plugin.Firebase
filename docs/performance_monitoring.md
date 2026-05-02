# Performance Monitoring

Firebase [Performance Monitoring](https://firebase.google.com/docs/perf-mon) helps collect app performance data and supports custom code traces and custom HTTP network request metrics.

## Installation
### Nuget
[![NuGet](https://img.shields.io/nuget/v/plugin.firebase.performancemonitoring.svg?maxAge=86400&style=flat)](https://www.nuget.org/packages/Plugin.Firebase.PerformanceMonitoring/)

> Install-Package Plugin.Firebase.PerformanceMonitoring

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup).
- Use Firebase's official [Performance Monitoring setup docs](https://firebase.google.com/docs/perf-mon/get-started) for Firebase Console and platform configuration.
- The native Firebase Performance Monitoring SDK can start automatic collection when linked into the app. If collection must be disabled before startup, configure Firebase's documented Android manifest or iOS plist disable keys before initialization.
- On Android, `Plugin.Firebase.PerformanceMonitoring` uses `Xamarin.Firebase.Perf` v121.0.0, which maps to Firebase Android BoM 33.0.0.

## Usage

```c#
using System.Net.Http;
using Plugin.Firebase.PerformanceMonitoring;

CrossFirebasePerformanceMonitoring.Current.IsDataCollectionEnabled = true;

var trace = CrossFirebasePerformanceMonitoring.Current.NewTrace("load_items");
trace.PutAttribute("screen", "items");
trace.Start();
trace.PutMetric("item_count", 10);
trace.IncrementMetric("item_count", 1);
trace.Stop();

var httpMetric = CrossFirebasePerformanceMonitoring.Current.NewHttpMetric(
    "https://example.com/items",
    HttpMethod.Get);
httpMetric.Start();
httpMetric.SetHttpResponseCode(200);
httpMetric.SetResponseContentType("application/json");
httpMetric.SetResponsePayloadSize(1024);
httpMetric.Stop();
```

Take a look at the official Firebase documentation for [custom code traces](https://firebase.google.com/docs/perf-mon/custom-code-traces) and [custom network request metrics](https://firebase.google.com/docs/perf-mon/custom-network-traces), because Plugin.Firebase keeps the API close to the native SDKs.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebasePerformanceMonitoring.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/PerformanceMonitoring/Shared/IFirebasePerformanceMonitoring.cs)
- [tests/.../PerformanceMonitoringFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/PerformanceMonitoring/PerformanceMonitoringFixture.cs)

## Release notes
- Version 4.0.0
  - Initial Firebase Performance Monitoring support for collection control, custom code traces, and custom HTTP metrics.
