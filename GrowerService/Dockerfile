# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

# Build image for restoring and compiling
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GrowerService.csproj", "./"]
RUN dotnet restore "GrowerService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "GrowerService.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "GrowerService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GrowerService.dll"]
