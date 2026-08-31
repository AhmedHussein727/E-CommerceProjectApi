# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so the layer caches when only source changes.
COPY ECommerce.sln ./
COPY ECommerce.API/ECommerce.Web.csproj                     ECommerce.API/
COPY ECommerce.Domain/ECommerce.Domain.csproj               ECommerce.Domain/
COPY ECommerce.Persistence/ECommerce.Persistence.csproj     ECommerce.Persistence/
COPY ECommerce.Presentation/ECommerce.Presentation.csproj   ECommerce.Presentation/
COPY ECommerce.Services/ECommerce.Services.csproj           ECommerce.Services/
COPY ECommerce.Services.Abstraction/ECommerce.Services.Abstraction.csproj ECommerce.Services.Abstraction/
COPY ECommerce.Shared/ECommerce.Shared.csproj               ECommerce.Shared/
RUN dotnet restore ECommerce.sln

COPY . .
RUN dotnet publish ECommerce.API/ECommerce.Web.csproj -c Release -o /app/publish

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Most PaaS providers inject the port to bind on.
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Run as the non-root user shipped with the base image.
USER $APP_UID

ENTRYPOINT ["dotnet", "ECommerce.Web.dll"]
