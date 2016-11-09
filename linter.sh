#!/bin/bash -x

# Abort the script if something fails
set -o errexit -o nounset

error=0
dotnet restore /app/src/Accounts/project.json || error=1
dotnet build /app/src/Accounts/project.json || error=1
rm -r /app/src/Accounts/{bin,obj}
exit $error