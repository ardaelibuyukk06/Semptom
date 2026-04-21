# ── Build aşaması ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala ve restore et (cache katmanı)
COPY SemptomAnalizApp.Core/SemptomAnalizApp.Core.csproj       SemptomAnalizApp.Core/
COPY SemptomAnalizApp.Data/SemptomAnalizApp.Data.csproj       SemptomAnalizApp.Data/
COPY SemptomAnalizApp.Service/SemptomAnalizApp.Service.csproj SemptomAnalizApp.Service/
COPY SemptomAnalizApp.Web/SemptomAnalizApp.Web.csproj         SemptomAnalizApp.Web/

RUN dotnet restore SemptomAnalizApp.Web/SemptomAnalizApp.Web.csproj

# Tüm kaynak kodu kopyala
COPY . .

# Publish et
RUN dotnet publish SemptomAnalizApp.Web/SemptomAnalizApp.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime aşaması ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Railway PORT env değişkenini kullan
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "SemptomAnalizApp.Web.dll"]
