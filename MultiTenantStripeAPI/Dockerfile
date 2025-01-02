# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5005

# Build image for restoring and compiling
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["src/MultiTenantStripeAPI.Api/MultiTenantStripeAPI.Api.csproj", "src/MultiTenantStripeAPI.Api/"]
COPY ["src/MultiTenantStripeAPI.Application/MultiTenantStripeAPI.Application.csproj", "src/MultiTenantStripeAPI.Application/"]
COPY ["src/MultiTenantStripeAPI.Domain/MultiTenantStripeAPI.Domain.csproj", "src/MultiTenantStripeAPI.Domain/"]
COPY ["src/MultiTenantStripeAPI.Infrastructure/MultiTenantStripeAPI.Infrastructure.csproj", "src/MultiTenantStripeAPI.Infrastructure/"]
COPY ["src/MultiTenantStripeAPI.Shared/MultiTenantStripeAPI.Shared.csproj", "src/MultiTenantStripeAPI.Shared/"]

# Restore dependencies
RUN dotnet restore "src/MultiTenantStripeAPI.Api/MultiTenantStripeAPI.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build the API project
WORKDIR "/src/src/MultiTenantStripeAPI.Api"
RUN dotnet build "MultiTenantStripeAPI.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MultiTenantStripeAPI.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MultiTenantStripeAPI.Api.dll"]
