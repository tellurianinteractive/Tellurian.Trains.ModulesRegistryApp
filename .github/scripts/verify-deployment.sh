#!/usr/bin/env bash
# Kudu can report "Deployment successful" without copying the files (its deployment script crashed, or KuduSync stopped at
# a locked file), so compare the deployed main assembly with the one in the build output.
# Usage: verify-deployment.sh <publish folder>   (needs an Azure CLI login with access to the app's Kudu site)
set -u
publish="$1"
token=$(az account get-access-token --resource https://management.core.windows.net/ --query accessToken --output tsv)
expected=$(sha256sum "$publish/ModulesRegistry.dll" | cut -d ' ' -f 1)
for attempt in 1 2 3 4 5 6; do
  rm -f deployed.dll
  curl --silent --show-error --fail --header "Authorization: Bearer $token" \
    --output deployed.dll "https://moduleregistry.scm.azurewebsites.net/api/vfs/site/wwwroot/ModulesRegistry.dll" || true
  actual=$( [ -f deployed.dll ] && sha256sum deployed.dll | cut -d ' ' -f 1 )
  if [ "$actual" = "$expected" ]; then echo "Deployed ModulesRegistry.dll matches the build output."; exit 0; fi
  echo "Attempt $attempt: deployed ModulesRegistry.dll doesn't match yet, waiting..."
  sleep 10
done
echo "The deployed ModulesRegistry.dll differs from the build output: the files were not deployed."
exit 1