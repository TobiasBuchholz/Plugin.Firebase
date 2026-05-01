#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
project="$repo_root/tests/Plugin.Firebase.IntegrationTests/Plugin.Firebase.IntegrationTests.csproj"
obj_dir="$repo_root/tests/Plugin.Firebase.IntegrationTests/obj/Release/net9.0-android"
acw_map="$obj_dir/acw-map.txt"
manifest="$obj_dir/AndroidManifest.xml"
managed_type="Plugin.Firebase.CloudMessaging.Platforms.Android.MyFirebaseMessagingService"
messaging_action="com.google.firebase.MESSAGING_EVENT"

dotnet build "$project" \
  -c Release \
  -f net9.0-android \
  /p:TrimMode=full \
  /p:AndroidPackageFormat=apk

python3 - "$acw_map" "$manifest" "$managed_type" "$messaging_action" <<'PY'
from pathlib import Path
import sys
import xml.etree.ElementTree as ET

acw_map = Path(sys.argv[1])
manifest = Path(sys.argv[2])
managed_type = sys.argv[3]
messaging_action = sys.argv[4]
android_ns = "{http://schemas.android.com/apk/res/android}"

if not acw_map.exists():
    raise SystemExit(f"Android callable-wrapper map was not generated: {acw_map}")

if not manifest.exists():
    raise SystemExit(f"Android manifest was not generated: {manifest}")

java_type = None
for line in acw_map.read_text(encoding="utf-8").splitlines():
    managed, separator, java = line.partition(";")
    if not separator:
        continue

    if managed.split(",", 1)[0].strip() == managed_type:
        java_type = java.strip()
        break

if not java_type:
    raise SystemExit(f"{managed_type} was not preserved in {acw_map}")

root = ET.parse(manifest).getroot()
services = [
    service
    for service in root.findall(".//service")
    if service.attrib.get(f"{android_ns}name") == java_type
]

if not services:
    raise SystemExit(f"{java_type} was not registered as a service in {manifest}")

service = services[0]
exported = service.attrib.get(f"{android_ns}exported")
if exported != "false":
    raise SystemExit(
        f"{java_type} should be registered with android:exported=\"false\", found {exported!r}"
    )

actions = [
    action.attrib.get(f"{android_ns}name")
    for action in service.findall("./intent-filter/action")
]
if messaging_action not in actions:
    raise SystemExit(
        f"{java_type} is missing the {messaging_action} intent action in {manifest}"
    )

print(f"Validated {managed_type} as {java_type} with android:exported=\"false\".")
PY
