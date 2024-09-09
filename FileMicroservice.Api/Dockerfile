FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FileMicroservice.Api/FileMicroservice.Api.csproj", "FileMicroservice.Api/"]
RUN dotnet restore "FileMicroservice.Api/FileMicroservice.Api.csproj"
COPY . .
WORKDIR "/src/FileMicroservice.Api"
RUN dotnet build "FileMicroservice.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FileMicroservice.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileMicroservice.Api.dll"]
