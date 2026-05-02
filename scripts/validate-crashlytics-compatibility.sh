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
projects=(
  "$repo_root/src/Crashlytics/Crashlytics.csproj"
  "$repo_root/src/Bundled/Bundled.csproj"
)

for version in "${versions[@]}"; do
  datastore_version="1.1.1.8"
  case "$version" in
    119.4.4|120.0.0)
      datastore_version="1.1.7"
      ;;
    120.0.5)
      datastore_version="1.2.1"
      ;;
  esac

  if [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then
    echo "::group::Validate Xamarin.Firebase.Crashlytics $version with Xamarin.AndroidX.DataStore $datastore_version"
  else
    echo "Validating Xamarin.Firebase.Crashlytics $version with Xamarin.AndroidX.DataStore $datastore_version"
  fi

  for project in "${projects[@]}"; do
    dotnet build "$project" \
      -c Release \
      -f net9.0-android \
      -m:1 \
      --disable-build-servers \
      -p:UseSharedCompilation=false \
      -p:XamarinFirebaseCrashlyticsVersion="$version" \
      -p:XamarinAndroidXDataStoreVersion="$datastore_version"
  done

  dotnet build-server shutdown >/dev/null 2>&1 || true

  if [[ "${GITHUB_ACTIONS:-}" == "true" ]]; then
    echo "::endgroup::"
  fi
done

echo "Validated Crashlytics Android compatibility for ${#versions[@]} package versions."
