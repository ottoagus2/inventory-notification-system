FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create directory for SQLite database
RUN mkdir -p /app/data

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NotificationService.csproj", "."]
RUN dotnet restore "./NotificationService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set permissions for data directory
RUN chmod 777 /app/data

ENTRYPOINT ["dotnet", "NotificationService.dll"]