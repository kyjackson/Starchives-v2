#!/usr/bin/env bash
set -euo pipefail

# Defaults if not provided by the caller
RUNTIME="${RUNTIME:-linux-x64}"
OUT_DIR="${OUT_DIR:-out}"
PROJECT="${PROJECT:-Starchives/Starchives.csproj}"

echo "== Publish app =="
dotnet publish "$PROJECT" \
  -c Release \
  -r "$RUNTIME" \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishReadyToRun=true \
  -o "$OUT_DIR/app"

echo "Publish complete → $OUT_DIR/app"
