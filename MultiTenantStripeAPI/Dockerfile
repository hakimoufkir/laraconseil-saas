# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5005

# Build image for restoring and compiling
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MultiTenantStripeAPI.csproj", "./"]
RUN dotnet restore "MultiTenantStripeAPI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "MultiTenantStripeAPI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MultiTenantStripeAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MultiTenantStripeAPI.dll"]
