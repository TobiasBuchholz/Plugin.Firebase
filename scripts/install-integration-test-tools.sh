#!/usr/bin/env bash
set -euo pipefail

firebase_tools_version="${FIREBASE_TOOLS_VERSION:-15.15.0}"
xharness_version="${XHARNESS_VERSION:-11.0.0-prerelease.26224.1}"

npm install --global "firebase-tools@${firebase_tools_version}"
dotnet tool install --global Microsoft.DotNet.XHarness.CLI \
  --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json \
  --version "${xharness_version}"
