#!/bin/bash

# Load environment variables from .env.prod
set -o allexport
source ../.env.prod
set +o allexport

# Function to update a container app
update_container_app() {
    local app_name=$1
    local image=$2
    shift 2
    local env_vars=("$@")

    echo "Updating $app_name with image $image..."

    # Construct Azure CLI command to update the container app
    az containerapp update \
        --name "$app_name" \
        --image "$image" \
        --set-env-vars $(printf "%s " "${env_vars[@]}") \
        --resource-group lcrg

    if [ $? -eq 0 ]; then
        echo "$app_name updated successfully!"
    else
        echo "Failed to update $app_name"
        exit 1
    fi
}

# Helper function to remove surrounding quotes from a value
strip_quotes() {
    local value="$1"
    echo "${value%\"}" | sed 's/^\"//'
}

# Helper function to format environment variables
wrap_env_var() {
    local key=$1
    local value=$(strip_quotes "$2")
    echo "$key='$value'"
}

# Update API Gateway
update_container_app "apigateway" "$APIGATEWAY_IMAGE" \
    "$(wrap_env_var "APIGATEWAY_PORT" "$APIGATEWAY_PORT")" \
    "$(wrap_env_var "SERVICE_PORT" "$APIGATEWAY_PORT")" \
    "$(wrap_env_var "ASPNETCORE_ENVIRONMENT" "$ASPNETCORE_ENVIRONMENT")"

# Update MultiTenant API
update_container_app "multitenant-api" "$MULTITENANT_API_IMAGE" \
    "$(wrap_env_var "MULTITENANT_API_PORT" "$MULTITENANT_API_PORT")" \
    "$(wrap_env_var "SERVICE_PORT" "$MULTITENANT_API_PORT")" \
    "$(wrap_env_var "ConnectionStrings__AzureServiceBus" "$AZURE_SERVICE_BUS_CONNECTION_STRING")" \
    "$(wrap_env_var "ConnectionStrings__DefaultSQLConnection" "$DEFAULT_SQL_CONNECTION")" \
    "$(wrap_env_var "AZURE_COMMUNICATION_CONNECTION_STRING" "$AZURE_COMMUNICATION_CONNECTION_STRING")" \
    "$(wrap_env_var "AZURE_COMMUNICATION_SENDER_EMAIL" "$AZURE_COMMUNICATION_SENDER_EMAIL")" \
    "$(wrap_env_var "Stripe__SecretKey" "$STRIPE_SECRET_KEY")" \
    "$(wrap_env_var "Stripe__PublishableKey" "$STRIPE_PUBLISHABLE_KEY")" \
    "$(wrap_env_var "Stripe__WebhookSecret" "$STRIPE_WEBHOOK_SECRET")"

# Update Messager Service
update_container_app "messager-service" "$MESSAGER_SERVICE_IMAGE" \
    "$(wrap_env_var "MESSAGER_SERVICE_PORT" "$MESSAGER_SERVICE_PORT")" \
    "$(wrap_env_var "SERVICE_PORT" "$MESSAGER_SERVICE_PORT")" \
    "$(wrap_env_var "ASPNETCORE_ENVIRONMENT" "$ASPNETCORE_ENVIRONMENT")" \
    "$(wrap_env_var "ConnectionStrings__AzureServiceBus" "$AZURE_SERVICE_BUS_CONNECTION_STRING")" \
    "$(wrap_env_var "AzureCommunicationServices__ConnectionString" "$AZURE_COMMUNICATION_CONNECTION_STRING")" \
    "$(wrap_env_var "AzureCommunicationServices__SenderEmail" "$AZURE_COMMUNICATION_SENDER_EMAIL")"

# Update Subscription App (Angular Frontend)
update_container_app "webapp" "$WEBAPP_IMAGE" \
    "$(wrap_env_var "PORT" "$WEBAPP_PORT")" \
    "$(wrap_env_var "ANGULAR_BUILD_CONFIGURATION" "$ANGULAR_BUILD_CONFIGURATION")"

# Update Keycloak Service API
update_container_app "keycloak-service-api" "$KEYCLOAK_SERVICE_API_IMAGE" \
    "$(wrap_env_var "SERVICE_PORT" "$KEYCLOAK_SERVICE_API_PORT")" \
    "$(wrap_env_var "ConnectionStrings__AzureServiceBus" "$AZURE_SERVICE_BUS_CONNECTION_STRING")" \
    "$(wrap_env_var "ASPNETCORE_ENVIRONMENT" "$ASPNETCORE_ENVIRONMENT")"

# Add new containers here as needed, following the same pattern
# Example for adding a new container:
# update_container_app "new-container" "$NEW_CONTAINER_IMAGE" \
#    "$(wrap_env_var "ENV_VAR_NAME" "$ENV_VAR_VALUE")"

echo "All container apps updated successfully!"