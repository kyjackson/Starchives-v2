#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${DO_TOKEN:-}" ]]; then
  echo "ERROR: DO_TOKEN env var is required" >&2
  exit 1
fi

echo "== Install doctl =="
curl -sL https://github.com/digitalocean/doctl/releases/download/v1.118.0/doctl-1.118.0-linux-amd64.tar.gz | tar -xz
sudo mv doctl /usr/local/bin/

echo "== Auth to DigitalOcean =="
doctl auth init -t "$DO_TOKEN"

echo "== Login to DOCR =="
doctl registry login
