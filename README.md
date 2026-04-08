# SemptomAnaliz

Bireysel sağlık farkındalığı için semptom benzerlik analiz uygulaması.  
ASP.NET Core 9 MVC + PostgreSQL (Supabase) + Clean Architecture.

**Hazırlayanlar:** Arda Elibüyük, Furkan Taşdelen — Bilgisayar Programcılığı  
**Tarih:** Nisan 2026

> **Önemli:** Bu uygulama tıbbi teşhis veya tavsiye vermez.
> Sonuçlar yalnızca istatistiksel benzerlik analizidir.

---

## Klasör Yapısı

```
SemptomAnalizApp/
├── docs/                            # Proje belgeleri
│   ├── GereksinimAnalizi.md         # Gereksinim analizi dokümanı
│   ├── ModulerTasarim.md            # Modüler sistem tasarımı
│   └── UML/
│       ├── UseCaseDiagram.puml      # Use Case diyagramı (PlantUML)
│       └── ClassDiagram.puml        # Sınıf diyagramı (PlantUML)
│
├── SemptomAnalizApp.Core/           # Domain katmanı (Entity, Enum, Interface)
│   ├── Entities/
│   ├── Enums/
│   └── Interfaces/
│
├── SemptomAnalizApp.Data/           # Veri erişim katmanı
│   ├── Migrations/
│   ├── Repositories/
│   ├── AppDbContext.cs
│   ├── DbSeeder.cs
│   └── UnitOfWork.cs
│
├── SemptomAnalizApp.Service/        # İş mantığı katmanı
│   ├── Interfaces/                  # IAnalizService
│   └── Services/                    # AnalizMotoru (Naive Bayes)
│
├── SemptomAnalizApp.Web/            # Sunum katmanı (MVC)
│   ├── Controllers/
│   ├── ViewModels/
│   ├── Views/
│   ├── wwwroot/
│   ├── Program.cs
│   └── appsettings.json
│
├── SemptomAnalizApp.Tests/          # Birim testler (27 xUnit testi)
├── SemptomAnalizApp.sln
├── README.md
└── .gitignore
```

---

## OOP Özellikleri

| OOP Kavramı | Uygulama |
|-------------|----------|
| **Kalıtım** | `BaseEntity` abstract sınıf → `AnalizOturumu`, `Semptom`, `Hastalik` vb. türev sınıflar |
| **Kalıtım** | `Kullanici` → `IdentityUser`'dan miras alır |
| **Polymorphism** | `IAnalizService` arayüzü → `AnalizMotoru` implementasyonu |
| **Polymorphism** | `IGenericRepository<T>` → `GenericRepository<T>` implementasyonu |
| **Encapsulation** | `AnalizMotoru` içindeki algoritma detayları `private` metodlarla gizli |
| **Encapsulation** | Repository'ler `protected readonly DbSet<T>` ile korunmuş |

---

## Mimari

```
Web (Sunum) → Service (İş Mantığı) → Data (Veri Erişim) → Core (Domain)
```

> Her katman yalnızca bir alttaki katmana bağımlıdır.  
> Core hiçbir projeye bağımlı değildir (Dependency Inversion).

### Analiz Motoru

Naive Bayes log-likelihood tabanlı semptom benzerlik hesabı:

```
log_posterior(D) = log(prior) + Σ log(P(Si|D) / P(Si|¬D)) + modifiers
```

Sonuç softmax normalizasyonuyla 0–100 arasında **benzerlik skoruna** dönüştürülür.  
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

`appsettings.json` içinde düzenle:

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

### 3. Çalıştır

```bash
dotnet run --project SemptomAnalizApp.Web
```

Uygulama adresleri:
- **https://localhost:7131**
- **http://localhost:5075**

İlk çalıştırmada:
- Veritabanı migration'ları otomatik uygulanır
- 42 hastalık + semptom kataloğu otomatik yüklenir
- Admin kullanıcısı oluşturulur: `admin@semptomanaliz.com`

### 4. Testleri Çalıştır

```bash
dotnet test
```

27 birim testi; tamamı başarılı çalışmalıdır.

---

## Güvenlik

- HTTPS / HSTS zorunlu (production)
- Security headers: X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy
- Rate limiting: 10 analiz/dakika per kullanıcı/IP
- CSRF koruması: AntiForgery token tüm formlarda
- Şifreler bcrypt hash olarak saklanır
- KVKK: Kayıt sırasında açık rıza + tıbbi sorumluluk reddi onayı

---

## Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| Web Çatısı | ASP.NET Core 9 MVC |
| ORM | Entity Framework Core 9 + Npgsql |
| Veritabanı | PostgreSQL (Supabase) |
| Kimlik Doğrulama | ASP.NET Core Identity |
| Ön Yüz | Bootstrap 5 dark tema + Chart.js 4 + SweetAlert2 |
| İkonlar | Bootstrap Icons 1.11 |
| Test | xUnit |

---

## KVKK

Uygulama Türkiye'de yürürlükteki 6698 sayılı KVKK kapsamında işletilmektedir.  
Aydınlatma metni: `/Home/Privacy`
