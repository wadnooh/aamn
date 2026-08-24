#!/bin/sh
set -e
ROOT=/home/u798103903/domains/wadnooh.com/public_html
mkdir -p "$ROOT/images" "$ROOT/data"
cd /tmp
curl -fsSL -o w.zip "https://sacrifice-logged-montreal-input.trycloudflare.com/deploy-pack/site.zip"
unzip -o w.zip -d "$ROOT"
chmod -R a+r "$ROOT"
ls -la "$ROOT"
ls -la "$ROOT/data" || true
