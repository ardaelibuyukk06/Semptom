# SemptomAnaliz

Bireysel sağlık farkındalığı için semptom benzerlik analiz uygulaması.
ASP.NET Core 9 MVC + PostgreSQL (Supabase) + Clean Architecture.

> **Önemli:** Bu uygulama tıbbi teşhis veya tavsiye vermez.
> Sonuçlar yalnızca istatistiksel benzerlik analizidir.

---

## Mimari

```
SemptomAnalizApp/
├── SemptomAnalizApp.Core/      # Entity'ler, enum'lar, interface'ler
├── SemptomAnalizApp.Data/      # EF Core DbContext, migrations, seeder
├── SemptomAnalizApp.Service/   # Naive Bayes analiz motoru, business logic
└── SemptomAnalizApp.Web/       # ASP.NET Core MVC, controller'lar, view'lar
```

### Analiz Motoru

Naive Bayes log-likelihood tabanlı semptom benzerlik hesabı:

```
log_posterior(D) = log(prior) + Σ log(P(Si|D) / P(Si|¬D)) + modifiers
```

Sonuç softmax normalizasyonuyla 0–100 arasında **benzerlik skoru**na dönüştürülür.
Bu skor istatistiksel benzerlik gösterir; kalibre edilmiş olasılık değildir.

---

## Hızlı Başlangıç

### Gereksinimler

- .NET 9 SDK
- PostgreSQL veritabanı (önerilir: [Supabase](https://supabase.com) ücretsiz tier)

### 1. Repo'yu klonla

```bash
git clone <repo-url>
cd SemptomAnalizApp
```

### 2. Konfigürasyon

```bash
cp SemptomAnalizApp.Web/appsettings.example.json SemptomAnalizApp.Web/appsettings.json
```

`appsettings.json` içinde aşağıdaki değerleri düzenle:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "User Id=...;Password=...;Server=...;Port=5432;Database=postgres;"
  },
  "Seed": {
    "AdminPassword": "GucluBirSifre123!"
  }
}
```

> **Güvenlik notu:** Production'da bağlantı dizesini ve admin şifresini
> [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
> veya environment variable olarak saklayın:
>
> ```bash
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."
> dotnet user-secrets set "Seed:AdminPassword" "..."
> ```

### 3. Çalıştır

```bash
dotnet run --project SemptomAnalizApp.Web
```

Uygulama şu adreslerde çalışır:

- **https://localhost:7131**
- **http://localhost:5075**

İlk çalıştırmada:
- Veritabanı migration'ları otomatik uygulanır
- 42 hastalık + semptom kataloğu otomatik yüklenir (DbSeeder)
- Admin kullanıcısı oluşturulur (`admin@semptomanaliz.com`)

---

## Veritabanı Sıfırlama

Seeder yeniden çalıştırılması gerekiyorsa (örn. hastalık verisi temizleme):

```sql
DELETE FROM "HastalikSemptomlar";
DELETE FROM "Hastaliklar";
```

Sonra uygulamayı yeniden başlat; seeder 42 hastalığı yeniden yükler.

---

## Deployment (Production)

### Environment Variables

| Değişken | Açıklama |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL bağlantı dizesi |
| `Seed__AdminPassword` | İlk admin kullanıcı şifresi |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### Docker (örnek)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY publish/ .
ENTRYPOINT ["dotnet", "SemptomAnalizApp.Web.dll"]
```

```bash
dotnet publish SemptomAnalizApp.Web -c Release -o publish/
docker build -t semptomanaliz .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Seed__AdminPassword="..." \
  -e ASPNETCORE_ENVIRONMENT=Production \
  semptomanaliz
```

### Render / Railway / Fly.io

1. Repoyu bağla
2. Environment variable'ları ayarla (yukarıdaki tablo)
3. Build komutu: `dotnet publish SemptomAnalizApp.Web -c Release -o publish/`
4. Start komutu: `dotnet publish/SemptomAnalizApp.Web.dll`

---

## Güvenlik

- HTTPS / HSTS zorunlu (production)
- Security headers: X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy
- Rate limiting: 10 analiz/dakika per kullanıcı/IP
- CSRF koruması: AntiForgery token tüm formlarda
- Şifreler bcrypt hash olarak saklanır (düz metin yok)
- KVKK: Kayıt sırasında açık rıza + tıbbi sorumluluk reddi onayı

---

## Teknoloji Yığını

| Katman | Teknoloji |
|---|---|
| Web framework | ASP.NET Core 9 MVC |
| ORM | Entity Framework Core 9 + Npgsql |
| Veritabanı | PostgreSQL (Supabase) |
| Auth | ASP.NET Core Identity |
| UI | Bootstrap 5 dark theme + Chart.js 4 + SweetAlert2 |
| İkonlar | Bootstrap Icons 1.11 |

---

## KVKK

Uygulama Türkiye'de yürürlükteki 6698 sayılı KVKK kapsamında işletilmektedir.
Aydınlatma metni: `/Home/Privacy`
