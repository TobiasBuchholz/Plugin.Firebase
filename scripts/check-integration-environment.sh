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
project_path="${repo_root}/tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj"
functions_dir="${repo_root}/tests/cloud-functions/functions"

errors=()
warnings=()
errors_count=0
warnings_count=0

add_error() {
  errors+=("$1")
  errors_count=$((errors_count + 1))
}

add_warning() {
  warnings+=("$1")
  warnings_count=$((warnings_count + 1))
}

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    add_error "Missing required command: $1"
  fi
}

require_file() {
  if [ ! -f "$1" ]; then
    add_error "Missing required file: $1"
  fi
}

require_dir() {
  if [ ! -d "$1" ]; then
    add_error "Missing required directory: $1"
  fi
}

is_port_listening() {
  local port="$1"
  if command -v lsof >/dev/null 2>&1; then
    lsof -nP -iTCP:"${port}" -sTCP:LISTEN >/dev/null 2>&1
    return
  fi

  if command -v nc >/dev/null 2>&1; then
    nc -z 127.0.0.1 "${port}" >/dev/null 2>&1
    return
  fi

  return 1
}

check_emulator_ports() {
  if [ "${ALLOW_BUSY_EMULATOR_PORTS:-0}" = "1" ]; then
    add_warning "Skipping emulator port availability checks because ALLOW_BUSY_EMULATOR_PORTS=1."
    return
  fi

  local busy_ports=()
  local busy_ports_count=0
  for port in 9099 8080 5001 9199; do
    if is_port_listening "${port}"; then
      busy_ports+=("${port}")
      busy_ports_count=$((busy_ports_count + 1))
    fi
  done

  if [ "${busy_ports_count}" -gt 0 ]; then
    add_error "Firebase emulator port(s) already in use: ${busy_ports[*]}. Stop the existing process or set ALLOW_BUSY_EMULATOR_PORTS=1."
  fi
}

check_common_environment() {
  require_command dotnet
  require_command node
  require_command npm
  require_command firebase

  require_file "${project_path}"
  require_file "${functions_dir}/package.json"
  require_dir "${functions_dir}/node_modules"
  require_file "${functions_dir}/lib/index.js"
  require_file "${repo_root}/tests/cloud-functions/scripts/seed-auth-emulator.js"

  check_emulator_ports
}

check_android_environment() {
  require_command adb

  if command -v adb >/dev/null 2>&1; then
    local device_id="${ANDROID_DEVICE_ID:-}"
    local online_device_count
    online_device_count="$(adb devices | awk 'NR > 1 && $2 == "device" { count++ } END { print count + 0 }')"

    if [ -n "${device_id}" ]; then
      if ! adb devices | awk -v device_id="${device_id}" 'NR > 1 && $1 == device_id && $2 == "device" { found = 1 } END { exit found ? 0 : 1 }'; then
        add_error "ANDROID_DEVICE_ID '${device_id}' is not an online adb device or emulator."
      fi
    elif [ "${online_device_count}" -eq 0 ]; then
      add_error "No online Android adb device or emulator was found."
    elif [ "${online_device_count}" -gt 1 ]; then
      add_error "Multiple online Android adb devices or emulators were found. Set ANDROID_DEVICE_ID to select one."
    fi
  fi
}

check_ios_environment() {
  require_command xcrun

  if [ -z "${DEVICE_ID:-}" ]; then
    add_error "DEVICE_ID must be set to an available iOS simulator UDID."
  elif command -v xcrun >/dev/null 2>&1; then
    if ! xcrun simctl list devices available | grep -F "${DEVICE_ID}" >/dev/null 2>&1; then
      add_error "DEVICE_ID '${DEVICE_ID}' is not an available iOS simulator."
    fi
  fi
}

case "${platform}" in
  android)
    check_common_environment
    check_android_environment
    ;;
  ios)
    check_common_environment
    check_ios_environment
    ;;
  *)
    usage
    exit 2
    ;;
esac

if [ "${warnings_count}" -gt 0 ]; then
  for warning in "${warnings[@]}"; do
    echo "warning: ${warning}" >&2
  done
fi

if [ "${errors_count}" -gt 0 ]; then
  echo "Integration environment preflight failed:" >&2
  for error in "${errors[@]}"; do
    echo " - ${error}" >&2
  done
  exit 1
fi

echo "Integration environment preflight passed for ${platform}."
