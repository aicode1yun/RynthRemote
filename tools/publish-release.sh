#!/usr/bin/env bash
# Publishes a built .ipa to the PUBLIC release feed (tombohar/rynthremote-releases) so
# SideStore can fetch it without auth: creates a GitHub Release with the .ipa and
# regenerates apps.json (the SideStore source). Runs in CI on the macOS runner.
# Requires env RELEASES_TOKEN (a PAT with Contents:write on the public releases repo).
#   usage: publish-release.sh <version> <path-to-ipa>
set -euo pipefail

VERSION="$1"
IPA="$2"
PUBREPO="tombohar/rynthremote-releases"
ASSET="RynthRemote.ipa"

cp "$IPA" "$ASSET"
SIZE=$(stat -f%z "$ASSET" 2>/dev/null || stat -c%s "$ASSET")
DATE=$(date +%Y-%m-%d)
URL="https://github.com/${PUBREPO}/releases/download/v${VERSION}/${ASSET}"

export GH_TOKEN="$RELEASES_TOKEN"

gh release create "v${VERSION}" "$ASSET" --repo "$PUBREPO" \
    --title "RynthRemote ${VERSION}" --notes "Automated build ${VERSION}." \
  || gh release upload "v${VERSION}" "$ASSET" --repo "$PUBREPO" --clobber

cat > apps.json <<JSON
{
  "name": "RynthRemote",
  "identifier": "com.tombohar.rynthremote.source",
  "apps": [
    {
      "name": "RynthRemote",
      "bundleIdentifier": "com.tombohar.rynthremote",
      "developerName": "Tom Bohar",
      "subtitle": "AC multibox monitor + remote",
      "localizedDescription": "Monitor and remote-control your Asheron's Call multi-boxes from your phone, via the RynthCore StatusAgent on your PC. Live health, kills/XP, components, equipped gear with appraisals, and one-tap toggles (nav/combat/buffing), profile switching, and utilities.",
      "iconURL": "https://raw.githubusercontent.com/${PUBREPO}/main/icon.png",
      "tintColor": "6366f1",
      "category": "utilities",
      "version": "${VERSION}",
      "versionDate": "${DATE}",
      "versionDescription": "Automated build ${VERSION}.",
      "downloadURL": "${URL}",
      "size": ${SIZE},
      "versions": [
        { "version": "${VERSION}", "date": "${DATE}", "localizedDescription": "Automated build ${VERSION}.", "downloadURL": "${URL}", "size": ${SIZE}, "minOSVersion": "15.0" }
      ]
    }
  ],
  "news": []
}
JSON

rm -rf pubrepo
git clone "https://x-access-token:${RELEASES_TOKEN}@github.com/${PUBREPO}.git" pubrepo
cp apps.json pubrepo/apps.json
cd pubrepo
git config user.name "RynthRemote CI"
git config user.email "ci@users.noreply.github.com"
git add apps.json
git commit -m "Update source feed to ${VERSION}" || echo "no change to commit"
git push
echo "Published v${VERSION} (${SIZE} bytes) to ${PUBREPO}."
