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
# The probe restores Crashlytics and Performance Monitoring together with the AndroidX versions
# declared by the package projects, as a consumer app would.
project="$repo_root/scripts/crashlytics-compatibility/CrashlyticsCompatibility.csproj"

for version in "${versions[@]}"; do
  if [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then
    echo "::group::Validate Xamarin.Firebase.Crashlytics $version"
  else
    echo "Validating Xamarin.Firebase.Crashlytics $version"
  fi

  dotnet build "$project" \
    -c Release \
    -f net10.0-android \
    -p:TargetFrameworks=net10.0-android \
    -m:1 \
    --disable-build-servers \
    -p:UseSharedCompilation=false \
    -p:TreatWarningsAsErrors=false \
    -p:XamarinFirebaseCrashlyticsVersion="$version"

  dotnet build-server shutdown >/dev/null 2>&1 || true

  if [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then
    echo "::endgroup::"
  fi
done

echo "Validated Crashlytics Android compatibility for ${#versions[@]} package versions."
