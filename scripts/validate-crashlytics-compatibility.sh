#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
versions=(
  "119.0.0"
  "119.0.3.2"
  "119.1.0"
  "119.4.4"
  "120.0.0"
  "120.0.5"
)
# PerformanceMonitoring is built alongside Crashlytics because it pins the AndroidX Lifecycle.Process version
# that has to co-resolve with the Crashlytics binding. Their combined restore is otherwise only exercised by
# the integration test app.
projects=(
  "$repo_root/src/Crashlytics/Crashlytics.csproj"
  "$repo_root/src/PerformanceMonitoring/PerformanceMonitoring.csproj"
)

for version in "${versions[@]}"; do
  datastore_version="1.1.1.8"
  lifecycle_process_version="2.8.7.4"
  case "$version" in
    119.4.4|120.0.0)
      datastore_version="1.1.7"
      ;;
    120.0.5)
      datastore_version="1.2.1"
      ;;
  esac
  case "$version" in
    120.0.0)
      lifecycle_process_version="2.9.1"
      ;;
    120.0.5)
      lifecycle_process_version="2.10.0.2"
      ;;
  esac

  if [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then
    echo "::group::Validate Xamarin.Firebase.Crashlytics $version with Xamarin.AndroidX.DataStore $datastore_version and Xamarin.AndroidX.Lifecycle.Process $lifecycle_process_version"
  else
    echo "Validating Xamarin.Firebase.Crashlytics $version with Xamarin.AndroidX.DataStore $datastore_version and Xamarin.AndroidX.Lifecycle.Process $lifecycle_process_version"
  fi

  for project in "${projects[@]}"; do
    dotnet build "$project" \
      -c Release \
      -f net10.0-android \
      -p:TargetFrameworks=net10.0-android \
      -m:1 \
      --disable-build-servers \
      -p:UseSharedCompilation=false \
      -p:TreatWarningsAsErrors=false \
      -p:XamarinFirebaseCrashlyticsVersion="$version" \
      -p:XamarinAndroidXDataStoreVersion="$datastore_version" \
      -p:XamarinAndroidXLifecycleProcessVersion="$lifecycle_process_version"
  done

  dotnet build-server shutdown >/dev/null 2>&1 || true

  if [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then
    echo "::endgroup::"
  fi
done

echo "Validated Crashlytics Android compatibility for ${#versions[@]} package versions."
