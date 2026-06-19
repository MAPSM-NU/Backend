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
COPY ["Gym App.API/Gym App.API.csproj", "Gym App.API/"]
COPY ["Gym App.Core/GymApp.Core.csproj", "Gym App.Core/"]
COPY ["Gym App.Application/GymApp.Application.csproj", "Gym App.Application/"]
COPY ["Gym App.Infrastructure/GymApp.Infrastructure.csproj", "Gym App.Infrastructure/"]
RUN dotnet restore "./Gym App.API/Gym App.API.csproj"
COPY . .
WORKDIR "/src/Gym App.API"
RUN dotnet build "./Gym App.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gym App.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gym_App.API.dll"]