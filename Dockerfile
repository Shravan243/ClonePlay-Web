# ──────────────── Build stage ────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

# copy everything and restore dependencies
COPY . ./
RUN dotnet restore

# publish for release
RUN dotnet publish -c Release -o /app/publish

# ──────────────── Runtime stage ────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview
WORKDIR /app

# copy compiled output from build stage
COPY --from=build /app/publish .

# start the app
ENTRYPOINT ["dotnet", "ClonePlayWeb.dll"]
