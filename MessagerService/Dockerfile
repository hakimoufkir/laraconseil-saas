# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5006

# Build image for restoring and compiling
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MessagerService.csproj", "./"]
RUN dotnet restore "MessagerService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "MessagerService.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MessagerService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessagerService.dll"]
