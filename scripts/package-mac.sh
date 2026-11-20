#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")/.."
RID="${1:-osx-arm64}"
case "$RID" in osx-arm64|osx-x64) ;; *) echo "Use osx-arm64 or osx-x64"; exit 1;; esac
APP="release/$RID/Petal.app"
dotnet publish -c Release -r "$RID" --self-contained true -o "$APP/Contents/MacOS"
mkdir -p "$APP/Contents/Resources"
cp packaging/Info.plist "$APP/Contents/Info.plist"
chmod +x "$APP/Contents/MacOS/Petal"
# Ad-hoc local signing; distribution notarization requires an Apple Developer identity.
codesign --force --deep --sign - "$APP"
echo "Built $APP"
