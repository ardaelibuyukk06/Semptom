# Modüler Sistem Tasarımı

**Proje:** SemptomAnaliz  
**Hazırlayanlar:** Arda Elibüyük, Furkan Taşdelen  
**Tarih:** Nisan 2026

---

## 1. Genel Mimari

SemptomAnaliz, **Clean Architecture** (Temiz Mimari) prensiplerine dayalı
4 katmanlı modüler bir yapıya sahiptir. Her katman bağımsız bir .NET projesidir;
yalnızca içe doğru bağımlılık (dependency inversion) kuralına uyulmuştur.

```
┌──────────────────────────────────────────────────────┐
│           SemptomAnalizApp.Web                       │
│  (Presentation Layer — Controller, View, ViewModel)  │
└───────────────────┬──────────────────────────────────┘
                    │ depends on
┌───────────────────▼──────────────────────────────────┐
│           SemptomAnalizApp.Service                   │
│  (Business Logic — AnalizMotoru, Interface)          │
└───────────────────┬──────────────────────────────────┘
                    │ depends on
┌───────────────────▼──────────────────────────────────┐
│           SemptomAnalizApp.Data                      │
│  (Data Access — DbContext, Migrations, Seeder)       │
└───────────────────┬──────────────────────────────────┘
                    │ depends on
┌───────────────────▼──────────────────────────────────┐
│           SemptomAnalizApp.Core                      │
│  (Domain — Entity, Enum, Interface tanımları)        │
└──────────────────────────────────────────────────────┘

           ↕ (bağımsız test projesi)
┌──────────────────────────────────────────────────────┐
│           SemptomAnalizApp.Tests                     │
│  (xUnit Birim Testleri — 27 test)                    │
└──────────────────────────────────────────────────────┘
```

---

## 2. Katman Detayları

### 2.1 Core Katmanı — `SemptomAnalizApp.Core`

En iç katman; hiçbir dış projeye bağımlılığı yoktur.

| Klasör | İçerik | Açıklama |
|--------|--------|----------|
| `Entities/` | `BaseEntity`, `Kullanici`, `SaglikProfili`, `AnalizOturumu`, `AnalizSonucu`, `Semptom`, `Hastalik`, `HastalikSemptom`, `AnalizSemptomu`, `OlasiDurum`, `SemptomKatalog` | Domain nesneleri |
| `Enums/` | `AciliyetSeviyesi`, `BmiKategori`, `SemptomKategorisi`, `SemptomSiddeti` | Sabit değer kümeleri |
| `Interfaces/` | `IGenericRepository<T>`, `IUnitOfWork` | Veri erişim sözleşmeleri |

**OOP Uygulaması:**
- `BaseEntity` abstract (soyut) sınıf → `AnalizOturumu`, `Semptom`, `Hastalik` vb. bu sınıftan **kalıtım** alır.
- `IGenericRepository<T>` generic interface → **Encapsulation** ve **Dependency Inversion** sağlar.

---

### 2.2 Data Katmanı — `SemptomAnalizApp.Data`

Veritabanı erişim katmanı. Yalnızca Core'a bağımlıdır.

| Dosya/Klasör | İçerik |
|-------------|--------|
| `AppDbContext.cs` | EF Core DbContext; tüm DbSet tanımları |
| `DbSeeder.cs` | 42 hastalık + semptom kataloğu + admin kullanıcı seed data |
| `Migrations/` | EF Core migration dosyaları (6 adet) |
| `Repositories/GenericRepository.cs` | `IGenericRepository<T>` implementasyonu |
| `UnitOfWork.cs` | Transaction yönetimi |

**OOP Uygulaması:**
- `GenericRepository<T>` sınıfı, `IGenericRepository<T>` interface'ini uygular → **Polymorphism**
- Tüm veri erişimi `protected readonly DbSet<T>` ile → **Encapsulation**

---

### 2.3 Service Katmanı — `SemptomAnalizApp.Service`

İş mantığı katmanı. Core ve Data'ya bağımlıdır.

| Dosya | İçerik |
|-------|--------|
| `Interfaces/IAnalizService.cs` | Analiz motoru sözleşmesi + `SemptomGirdisi` record |
| `Services/AnalizMotoru.cs` | Naive Bayes log-likelihood tabanlı analiz motoru |

**OOP Uygulaması:**
- `AnalizMotoru`, `IAnalizService` interface'ini uygular → **Polymorphism**
- Controller sadece `IAnalizService`'i tanır, implementasyonu bilmez → **Loose Coupling**
- Algoritma detayları `private` methodlarla gizlidir → **Encapsulation**

**Analiz Algoritması Özeti:**
```
log_posterior(D) = log(prior(D))
                + Σ log(P(Si|D) / P(Si|¬D))    [seçili semptomlar]
                + Σ 0.4×log(P(Si|D) / P(Si|¬D)) [yüksek ağırlıklı eksik semptomlar]
                + log(yaş_modifier × cinsiyet_modifier × süre_modifier × şiddet_modifier)

Sonuç → softmax normalizasyon → 0–100 göreli benzerlik skoru
```

---

### 2.4 Web Katmanı — `SemptomAnalizApp.Web`

Sunum katmanı. Tüm projelere bağımlıdır.

| Klasör | İçerik |
|--------|--------|
| `Controllers/` | `HomeController`, `AnalizController`, `HesapController`, `ProfilController`, `GecmisController`, `AdminController` |
| `ViewModels/` | `AnalizViewModels`, `HesapViewModels`, `ProfilViewModel`, `GecmisViewModel`, `DashboardViewModel` |
| `Views/` | Razor View dosyaları (her controller için ayrı klasör) |
| `Views/Shared/` | `_Layout.cshtml`, `_LoginPartial.cshtml` |
| `wwwroot/` | CSS, JS, Bootstrap, jQuery kütüphaneleri |
| `Program.cs` | DI container, middleware pipeline, uygulama başlatma |
| `appsettings.json` | Bağlantı dizesi, seed şifre yapılandırması |

---

### 2.5 Test Projesi — `SemptomAnalizApp.Tests`

| Dosya | İçerik |
|-------|--------|
| `AnalizMotoruTests.cs` | 27 xUnit birim testi |

**Test kapsamı:**
- `HesaplaBmi` — 7 senaryo (sıfır boy, null profil, tüm BMI kategorileri)
- `SkoraSeviyeAta` — 8 eşik değeri senaryosu (Normal/İzle/Dikkat/Acil)
- `OlusturImza` — 5 senaryo (uzunluk, küçük harf hex, sıra bağımsızlık, çakışma)
- `HesaplaAciliyetSkoru` — 5 senaryo (boş girdi, kritik semptom, yaşlı, kronik, max 100)

---

## 3. Bağımlılık Grafiği

```
Web ──────────────────────────► Service ──► Core
 │                                          ▲
 └──────────────────────────────► Data ─────┘
 
Tests ─────────────────────────► Service ──► Core
                                 └────────► Data
```

> Her ok bağımlılık yönünü gösterir. Core hiçbir projeye bağımlı değildir.

---

## 4. Güvenlik Modülü

`Program.cs` middleware pipeline'ında yapılandırılmıştır:

| Bileşen | Açıklama |
|---------|----------|
| ASP.NET Core Identity | Kullanıcı kaydı, giriş, bcrypt şifre hash |
| Cookie Authentication | 14 günlük sliding expiration |
| CSRF Token | Tüm POST formlarında `@Html.AntiForgeryToken()` |
| Rate Limiter | Analiz endpoint'i: 10 istek/dakika |
| Security Headers | X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy |
| Authorization | `[Authorize]` attribute ile controller bazlı erişim kontrolü |
| Role-based Auth | "Admin" ve "Kullanici" rolleri; Admin paneli yalnızca Admin'e açık |

---

## 5. Veritabanı Şeması (Özet)

```
Kullanicilar (ASP.NET Identity)
    └── SaglikProfilleri (1:1)
    └── AnalizOturumlari (1:N)
            └── AnalizSemptomlari (1:N) ──► SemptomKatalog ──► Semptomlar
            └── AnalizSonuclari (1:1)
                    └── OlasiDurumlar (1:N) ──► Hastaliklar

Hastaliklar
    └── HastalikSemptomlari (N:M junction) ──► Semptomlar
```

---

## 6. Klasör Yapısı

```
SemptomAnalizApp/
├── docs/                            # Proje belgeleri (bu klasör)
│   ├── GereksinimAnalizi.md         # Gereksinim analizi dokümanı
│   ├── ModulerTasarim.md            # Bu doküman
│   └── UML/
│       ├── UseCaseDiagram.puml      # Use Case diyagramı (PlantUML)
│       └── ClassDiagram.puml        # Sınıf diyagramı (PlantUML)
│
├── SemptomAnalizApp.Core/           # Domain katmanı
│   ├── Entities/                    # Entity sınıfları (BaseEntity + türevleri)
│   ├── Enums/                       # Enum tanımları
│   └── Interfaces/                  # Repository ve servis arayüzleri
│
├── SemptomAnalizApp.Data/           # Veri erişim katmanı
│   ├── Migrations/                  # EF Core migration dosyaları
│   ├── Repositories/                # Generic Repository implementasyonu
│   ├── AppDbContext.cs              # EF Core DbContext
│   ├── DbSeeder.cs                  # Seed data
│   └── UnitOfWork.cs                # Transaction yönetimi
│
├── SemptomAnalizApp.Service/        # İş mantığı katmanı
│   ├── Interfaces/                  # IAnalizService arayüzü
│   └── Services/                    # AnalizMotoru implementasyonu
│
├── SemptomAnalizApp.Web/            # Sunum katmanı
│   ├── Controllers/                 # MVC Controller'lar (6 adet)
│   ├── ViewModels/                  # ViewModel sınıfları
│   ├── Views/                       # Razor View'lar
│   │   ├── Admin/
│   │   ├── Analiz/
│   │   ├── Gecmis/
│   │   ├── Hesap/
│   │   ├── Home/
│   │   ├── Profil/
│   │   └── Shared/
│   ├── wwwroot/                     # Statik dosyalar (CSS, JS, kütüphaneler)
│   ├── Program.cs                   # Uygulama başlatma ve DI
│   └── appsettings.json             # Yapılandırma
│
├── SemptomAnalizApp.Tests/          # Birim testler
│   └── AnalizMotoruTests.cs         # 27 xUnit testi
│
├── SemptomAnalizApp.sln             # Visual Studio solution dosyası
├── README.md                        # Proje genel açıklaması
└── .gitignore                       # Git yoksayma kuralları
```

---

*Bu doküman SemptomAnaliz projesinin modüler sistem tasarımını açıklamaktadır.*  
*Son güncelleme: Nisan 2026*
