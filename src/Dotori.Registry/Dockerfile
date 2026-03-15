FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY Dotori.slnx ./
COPY src/Dotori.Core/Dotori.Core.csproj src/Dotori.Core/
COPY src/Dotori.Registry/Dotori.Registry.csproj src/Dotori.Registry/
RUN dotnet restore src/Dotori.Registry/Dotori.Registry.csproj

# Copy source and publish
COPY src/Dotori.Core/ src/Dotori.Core/
COPY src/Dotori.Registry/ src/Dotori.Registry/
RUN dotnet publish src/Dotori.Registry/Dotori.Registry.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Create data directory
RUN mkdir -p /data/packages

ENV ASPNETCORE_URLS=http://+:8080
ENV Registry__Mode=standalone
ENV Registry__StorageRoot=/data/packages
ENV ConnectionStrings__Default="Data Source=/data/registry.db"

EXPOSE 8080
VOLUME ["/data"]

ENTRYPOINT ["dotnet", "dotori-registry.dll"]
