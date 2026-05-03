# ── Build aşaması ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala ve restore et (cache katmanı)
COPY src/SemptomAnalizApp.Core/SemptomAnalizApp.Core.csproj       src/SemptomAnalizApp.Core/
COPY src/SemptomAnalizApp.Data/SemptomAnalizApp.Data.csproj       src/SemptomAnalizApp.Data/
COPY src/SemptomAnalizApp.Service/SemptomAnalizApp.Service.csproj src/SemptomAnalizApp.Service/
COPY src/SemptomAnalizApp.Web/SemptomAnalizApp.Web.csproj         src/SemptomAnalizApp.Web/

RUN dotnet restore src/SemptomAnalizApp.Web/SemptomAnalizApp.Web.csproj

# Tüm kaynak kodu kopyala
COPY . .

# Publish et
RUN dotnet publish src/SemptomAnalizApp.Web/SemptomAnalizApp.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime aşaması ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# Railway PORT env degiskeni runtime'da shell tarafindan cozulur.
ENTRYPOINT ["sh", "-c", "dotnet SemptomAnalizApp.Web.dll --urls http://+:${PORT:-8080}"]
