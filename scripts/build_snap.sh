#!/usr/bin/env bash
set -e

which snapcraft >/dev/null || (echo "Please install snapcraft first" && exit 1)
pushd snap
snapcraft clean build-and-install-console-frontend -s build
snapcraft clean build-and-install-console-frontend -s pull
snapcraft

