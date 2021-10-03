#!/bin/bash
export ASSEMBLY_FILE='TestCop.Plugin/Properties/AssemblyInfo.cs'
export TOP_LVL="$(git rev-parse --show-toplevel)"
export version=`git diff HEAD^..HEAD -- $TOP_LVL/$ASSEMBLY_FILE | \
grep "assembly: AssemblyVersion" | sed -s 's/[^0-9\.]//g'| sed 's/.$//' | tail -1`


# APPVEYOR_PULL_REQUEST_NUMBER
# APPVEYOR_BUILD_NUMBER