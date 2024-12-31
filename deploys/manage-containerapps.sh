#!/bin/bash

# Azure Resource Group
RESOURCE_GROUP="lcrg"

# Array of container apps to manage
declare -a CONTAINER_APPS=("apigateway" "subscription-app" "multitenant-api")

# Authenticate with Azure (credentials provided via GitHub Actions)
az login --service-principal --username "$AZURE_CLIENT_ID" --password "$AZURE_CLIENT_SECRET" --tenant "$AZURE_TENANT_ID"

# Function to update container apps
update_container_app() {
  local app_name="$1"
  echo "Updating container app: $app_name..."
  az containerapp update --name "$app_name" --resource-group "$RESOURCE_GROUP"
}

# Function to stop container apps
stop_container_app() {
  local app_name="$1"
  echo "Stopping container app: $app_name..."
  az containerapp revision deactivate --name "$app_name" --resource-group "$RESOURCE_GROUP"
}

# Function to start container apps
start_container_app() {
  local app_name="$1"
  echo "Starting container app: $app_name..."
  az containerapp revision activate --name "$app_name" --resource-group "$RESOURCE_GROUP"
}

# Iterate over the container apps and manage them
for app in "${CONTAINER_APPS[@]}"; do
  echo "Managing container app: $app"
  
  # Step 1: Update the container app
  update_container_app "$app"
  
  # Step 2: Stop the container app
  stop_container_app "$app"
  
  # Step 3: Start the container app
  start_container_app "$app"
  
  echo "Management of $app completed."
done

echo "All container apps have been updated, stopped, and started successfully!"
