#!/usr/bin/env bash
set -euo pipefail

firebase_tools_version="${FIREBASE_TOOLS_VERSION:-15.15.0}"

npm install --global "firebase-tools@${firebase_tools_version}"
