#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG AVPR_BUILD_REVISION=unknown
WORKDIR /src
COPY ["src/PackageRegistryService/PackageRegistryService.csproj", "src/PackageRegistryService/"]
RUN dotnet restore "./src/PackageRegistryService/./PackageRegistryService.csproj"
COPY . .
WORKDIR "/src/src/PackageRegistryService"
RUN dotnet build "./PackageRegistryService.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:SourceRevisionId=$AVPR_BUILD_REVISION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG AVPR_BUILD_REVISION=unknown
RUN dotnet publish "./PackageRegistryService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:SourceRevisionId=$AVPR_BUILD_REVISION

FROM base AS final
ARG AVPR_BUILD_REVISION=unknown
ARG AVPR_BUILD_CHANNEL=local
ARG AVPR_BUILD_CREATED
ENV AVPR_BUILD_REVISION=$AVPR_BUILD_REVISION \
    AVPR_BUILD_CHANNEL=$AVPR_BUILD_CHANNEL \
    AVPR_BUILD_CREATED=$AVPR_BUILD_CREATED
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PackageRegistryService.dll"]
