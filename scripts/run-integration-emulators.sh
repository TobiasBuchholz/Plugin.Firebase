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
integration_project="${repo_root}/tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj"

export FUNCTIONS_DISCOVERY_TIMEOUT="${FUNCTIONS_DISCOVERY_TIMEOUT:-30}"

quote() {
  printf '%q' "$1"
}

run_android() {
  local output_dir="${ANDROID_TEST_RESULTS_DIR:-${repo_root}/artifacts/test-results/android}"
  local device_id="${ANDROID_DEVICE_ID:-}"
  local command
  command="node scripts/seed-auth-emulator.js && "
  command+="dotnet test $(quote "${integration_project}") "
  command+="-c Debug -f net10.0-android "
  command+="-p:TargetFrameworks=net10.0-android "
  command+="--logger trx "
  command+="--results-directory $(quote "${output_dir}") "
  command+="-p:DeviceRunnersConnectionTimeout=600 "
  command+="-p:DeviceRunnersDataTimeout=600"
  if [ -n "${device_id}" ]; then
    command+=" -p:Device=$(quote "${device_id}")"
    command+=" -p:DeviceRunnersDevice=$(quote "${device_id}")"
  fi

  cd "${cloud_functions_dir}"
  firebase emulators:exec --project "${project_id}" --only "${emulators}" "${command}"
}

run_ios() {
  if [ -z "${DEVICE_ID:-}" ]; then
    echo "DEVICE_ID must be set to an available iOS simulator UDID." >&2
    exit 1
  fi

  local output_dir="${IOS_TEST_RESULTS_DIR:-${repo_root}/artifacts/test-results/ios}"
  local command
  command="node scripts/seed-auth-emulator.js && "
  command+="dotnet test $(quote "${integration_project}") "
  command+="-c Debug -f net10.0-ios "
  command+="-p:TargetFrameworks=net10.0-ios "
  command+="--logger trx "
  command+="--results-directory $(quote "${output_dir}") "
  command+="-p:RuntimeIdentifier=iossimulator-arm64 "
  command+="-p:EnableCodeSigning=false "
  command+="-p:DeviceRunnersDevice=$(quote "${DEVICE_ID}") "
  command+="-p:DeviceRunnersConnectionTimeout=600 "
  command+="-p:DeviceRunnersDataTimeout=600"

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
