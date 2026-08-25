# Build from the repository root:  docker build -f src/Portfolio.Api/Dockerfile -t portfolio-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Portfolio.Shared/Portfolio.Shared.csproj src/Portfolio.Shared/
COPY src/Portfolio.Api/Portfolio.Api.csproj src/Portfolio.Api/
RUN dotnet restore src/Portfolio.Api/Portfolio.Api.csproj

COPY src/ src/
RUN dotnet publish src/Portfolio.Api/Portfolio.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# Render sets PORT; keep the two in step (see render.yaml).
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# appsettings.json is baked into the image and cannot change at runtime, so watching it
# buys nothing -- and every watcher costs an inotify instance, a scarce resource shared
# across containers on small Linux hosts. Without this, startup intermittently dies with:
#   IOException: The configured user limit (128) on the number of inotify instances has been reached
ENV DOTNET_hostBuilder__reloadConfigOnChange=false

# Stateless by design: messages go straight out through Resend, nothing is written to disk.
ENTRYPOINT ["dotnet", "Portfolio.Api.dll"]
