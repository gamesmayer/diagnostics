#!/usr/bin/env sh
set -eu
tests=$(CDPATH='' cd "$(dirname "$0")" && pwd)
for test in "$tests"/*.test.sh; do sh "$test"; done
