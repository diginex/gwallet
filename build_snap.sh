#!/usr/bin/env bash
set -e

which snapcraft >/dev/null || (echo "Please install snapcraft first" && exit 1)

# simplest answer from stackoverflow to get the dir of this script (not the current dir)
# (see https://stackoverflow.com/a/337006 )
#DIR_OF_THIS_SCRIPT=`dirname $0`
#pushd $DIR_OF_THIS_SCRIPT/snap

snapcraft clean build-and-install-console-frontend -s build
snapcraft clean build-and-install-console-frontend -s pull
snapcraft

