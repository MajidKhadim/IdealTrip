# Use the official .NET 8 runtime image as a base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IdealTrip.csproj", "./"]
RUN dotnet restore "IdealTrip.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet publish "IdealTrip.csproj" -c Release -o /app/publish

# Runtime Stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IdealTrip.dll"]
