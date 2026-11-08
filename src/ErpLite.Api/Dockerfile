# Build context: repository root (needs global.json and Directory.*.props).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/ErpLite.Domain/ErpLite.Domain.csproj src/ErpLite.Domain/
COPY src/ErpLite.Application/ErpLite.Application.csproj src/ErpLite.Application/
COPY src/ErpLite.Infrastructure/ErpLite.Infrastructure.csproj src/ErpLite.Infrastructure/
COPY src/ErpLite.Api/ErpLite.Api.csproj src/ErpLite.Api/
RUN dotnet restore src/ErpLite.Api/ErpLite.Api.csproj
COPY src/ src/
RUN dotnet publish src/ErpLite.Api/ErpLite.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
# Non-root user provided by the official image.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ErpLite.Api.dll"]
