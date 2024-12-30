#!/bin/bash
set -e  # Exit immediately if a command exits with a non-zero status.

# Navigate to the deploys folder (optional if already in this folder)
cd "$(dirname "$0")"

# Azure Container Registry (ACR) name
ACR_NAME="lcrgacr.azurecr.io"

# Services and their paths (adjusted relative to the deploys folder)
declare -A services=(
  ["apigateway"]="../ApiGateway"
  ["grower-service"]="../GrowerService"
  ["station-service"]="../StationService"
  ["multitenant-api"]="../MultiTenantStripeAPI"
  ["messager-service"]="../MessagerService"
  ["subscription-app"]="../subscription-app"
)

# Build, tag, and push each service
for service in "${!services[@]}"; do
  echo "Building and tagging image for $service..."
  docker build -t "$ACR_NAME/$service:latest" "${services[$service]}"
  
  echo "Pushing image for $service to ACR..."
  docker push "$ACR_NAME/$service:latest"
done

echo "All images have been built, tagged with 'latest', and pushed to $ACR_NAME successfully!"
