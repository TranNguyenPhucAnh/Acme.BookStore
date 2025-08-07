# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
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

RUN dotnet tool install -g Volo.Abp.Cli && export PATH="$PATH:/root/.dotnet/tools"

RUN dotnet restore "./src/Acme.BookStore.HttpApi.Host/Acme.BookStore.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/src/Acme.BookStore.HttpApi.Host"
RUN dotnet build "./Acme.BookStore.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Acme.BookStore.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# ENTRYPOINT ["dotnet", "Acme.BookStore.HttpApi.Host.dll"]
ENTRYPOINT ["tail", "-f", "/dev/null"]
# The above ENTRYPOINT is used to keep the container running for debugging purposes.
