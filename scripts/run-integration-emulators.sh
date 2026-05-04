#!/usr/bin/env bash
set -euo pipefail

usage() {
  echo "Usage: $0 android|ios" >&2
}

if [ "$#" -ne 1 ]; then
  usage
  exit 2
fi

platform="$1"
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
project_id="${FIREBASE_PROJECT_ID:-demo-pluginfirebase-integrationtests}"
emulators="${FIREBASE_EMULATORS:-auth,firestore,functions,storage}"
cloud_functions_dir="${repo_root}/tests/cloud-functions"

export PATH="${PATH}:${HOME}/.dotnet/tools"
export FUNCTIONS_DISCOVERY_TIMEOUT="${FUNCTIONS_DISCOVERY_TIMEOUT:-30}"

quote() {
  printf '%q' "$1"
}

resolve_android_metadata() {
  if [ -z "${ANDROID_PACKAGE_ID:-}" ]; then
    ANDROID_PACKAGE_ID="$(dotnet msbuild "${repo_root}/tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj" \
      -getProperty:IntegrationTestsAndroidApplicationId \
      -p:TargetFramework=net9.0-android \
      | tr -d '\r\n')"
  fi

  if [ -z "${ANDROID_APK:-}" ]; then
    ANDROID_APK="$(find "${repo_root}/tests/Plugin.Firebase.IntegrationTests/bin/Debug/net9.0-android" \
      -name "${ANDROID_PACKAGE_ID}-Signed.apk" \
      -type f \
      -print \
      -quit)"
  fi

  if [ -z "${ANDROID_PACKAGE_ID}" ]; then
    echo "Could not resolve IntegrationTestsAndroidApplicationId." >&2
    exit 1
  fi

  if [ -z "${ANDROID_APK}" ]; then
    echo "Could not find signed Android APK for package '${ANDROID_PACKAGE_ID}'." >&2
    exit 1
  fi
}

run_android() {
  resolve_android_metadata

  local output_dir="${ANDROID_TEST_RESULTS_DIR:-${repo_root}/artifacts/test-results/android}"
  local command
  command="node scripts/seed-auth-emulator.js && "
  command+="xharness android test "
  command+="--timeout=00:10:00 "
  command+="--launch-timeout=00:10:00 "
  command+="--package-name $(quote "${ANDROID_PACKAGE_ID}") "
  command+="--instrumentation devicerunners.xharness.maui.XHarnessInstrumentation "
  command+="--app $(quote "${ANDROID_APK}") "
  command+="--output-directory $(quote "${output_dir}") "
  command+="--verbosity=Debug"

  cd "${cloud_functions_dir}"
  firebase emulators:exec --project "${project_id}" --only "${emulators}" "${command}"
}

run_ios() {
  if [ -z "${DEVICE_ID:-}" ]; then
    echo "DEVICE_ID must be set to an available iOS simulator UDID." >&2
    exit 1
  fi

  local app_path="${IOS_APP_PATH:-${repo_root}/tests/Plugin.Firebase.IntegrationTests/bin/Debug/net9.0-ios/iossimulator-arm64/Plugin.Firebase.IntegrationTests.app}"
  local output_dir="${IOS_TEST_RESULTS_DIR:-${repo_root}/artifacts/test-results/ios}"
  local command
  command="node scripts/seed-auth-emulator.js && "
  command+="xharness apple test "
  command+="--target ios-simulator-64 "
  command+="--device $(quote "${DEVICE_ID}") "
  command+="--timeout=00:10:00 "
  command+="--launch-timeout=00:10:00 "
  command+="--app $(quote "${app_path}") "
  command+="--output-directory $(quote "${output_dir}") "
  command+="--set-env=PLUGIN_FIREBASE_TEST_BACKEND=emulator"

  cd "${cloud_functions_dir}"
  firebase emulators:exec --project "${project_id}" --only "${emulators}" "${command}"
}

if [ "${SKIP_INTEGRATION_PREFLIGHT:-0}" != "1" ]; then
  "${repo_root}/scripts/check-integration-environment.sh" "${platform}"
fi

case "${platform}" in
  android)
    run_android
    ;;
  ios)
    run_ios
    ;;
  *)
    usage
    exit 2
    ;;
esac
