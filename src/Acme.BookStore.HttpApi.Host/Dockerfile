# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base stage for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# Set environment variable for non-root user (optional, as $APP_UID is typically 'app' with UID 1654)
ENV APP_UID=1654
WORKDIR /app
# Create certs directory for volume-mounted PFX file
RUN mkdir -p /app/certs && chmod 775 /app/certs
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["src/Acme.BookStore.HttpApi.Host/Acme.BookStore.HttpApi.Host.csproj", "src/Acme.BookStore.HttpApi.Host/"]
COPY ["src/Acme.BookStore.Application/Acme.BookStore.Application.csproj", "src/Acme.BookStore.Application/"]
COPY ["src/Acme.BookStore.Domain/Acme.BookStore.Domain.csproj", "src/Acme.BookStore.Domain/"]
COPY ["src/Acme.BookStore.Domain.Shared/Acme.BookStore.Domain.Shared.csproj", "src/Acme.BookStore.Domain.Shared/"]
COPY ["src/Acme.BookStore.Application.Contracts/Acme.BookStore.Application.Contracts.csproj", "src/Acme.BookStore.Application.Contracts/"]
COPY ["src/Acme.BookStore.BackgroundWorker/Acme.BookStore.BackgroundWorker.csproj", "src/Acme.BookStore.BackgroundWorker/"]
COPY ["src/Acme.BookStore.HttpApi/Acme.BookStore.HttpApi.csproj", "src/Acme.BookStore.HttpApi/"]
COPY ["src/Acme.BookStore.EntityFrameworkCore/Acme.BookStore.EntityFrameworkCore.csproj", "src/Acme.BookStore.EntityFrameworkCore/"]
RUN dotnet restore "./src/Acme.BookStore.HttpApi.Host/Acme.BookStore.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/src/Acme.BookStore.HttpApi.Host"
RUN dotnet build "./Acme.BookStore.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Acme.BookStore.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage for production
FROM base AS final
WORKDIR /app
# Copy published files with correct ownership
COPY --from=publish --chown=1654:1654 /app/publish .
# Set permissions for /app
RUN chmod -R 775 /app
# Switch to non-root user
USER $APP_UID
ENTRYPOINT ["dotnet", "Acme.BookStore.HttpApi.Host.dll"]