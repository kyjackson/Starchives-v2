#!/usr/bin/env bash
set -euo pipefail

RUNTIME="${RUNTIME:-linux-x64}"
OUT_DIR="${OUT_DIR:-out}"
PROJECT="${PROJECT:-Starchives/Starchives.csproj}"

echo "== Install EF tool (9.x) =="
dotnet tool install --global dotnet-ef --version 9.*
export PATH="$HOME/.dotnet/tools:$PATH"

echo "== Restore =="
dotnet restore "$PROJECT"

echo "== Build EF migration bundle (net9.0) =="
dotnet-ef migrations bundle \
  --project "$PROJECT" \
  --startup-project "$PROJECT" \
  --framework net9.0 \
  --self-contained \
  --target-runtime "$RUNTIME" \
  --output "$OUT_DIR/app/efbundle" -v

chmod +x "$OUT_DIR/app/efbundle"
echo "EF bundle ready → $OUT_DIR/app/efbundle"
