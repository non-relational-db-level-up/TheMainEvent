FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=1000
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MainEventBackend/MainEventBackend.csproj", "MainEventBackend/MainEventBackend.csproj"]

RUN dotnet restore "MainEventBackend/MainEventBackend.csproj"
COPY ./MainEventBackend ./MainEventBackend
WORKDIR "/src/MainEventBackend"

# Create /app/build directory and set permissions
RUN mkdir -p /app/build && chown -R $APP_UID:$APP_UID /app/build

RUN ls -a
RUN dotnet build "MainEventBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MainEventBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MainEventBackend.dll"]
