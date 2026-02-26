# -------------------------------
# Build Stage
# -------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first for better layer caching
COPY Backend/StargateAPI/StargateAPI.csproj Backend/StargateAPI/
RUN dotnet restore Backend/StargateAPI/StargateAPI.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish Backend/StargateAPI/StargateAPI.csproj -c Release -o /app/out

# -------------------------------
# Runtime Stage
# -------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out ./

# Render needs binding to all interfaces.
# We'll use 10000 and set Render's service port to 10000.
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "StargateAPI.dll"]