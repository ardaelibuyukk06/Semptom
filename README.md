# SemptomAnaliz

Bireysel sağlık farkındalığı için semptom benzerlik analiz uygulaması.  
ASP.NET Core 9 MVC + PostgreSQL (Supabase) + Clean Architecture.

**Hazırlayanlar:** Arda Elibüyük, Furkan Taşdelen — Bilgisayar Programcılığı  
**Tarih:** Nisan 2026

> **Önemli:** Bu uygulama tıbbi teşhis veya tavsiye vermez.
> Sonuçlar yalnızca istatistiksel benzerlik analizidir.

---

## Klasör Yapısı ve Belgeler

Proje, teslim kısıtları ve "Proje Klasör Yapısı" şablonuna birebir uyacak şekilde fiziksel olarak yapılandırılmıştır:

```
SemptomAnalizApp/
├── docs/                            # Akademik Belgeler
│   ├── GereksinimAnalizi.pdf        # Gereksinim Analizi Raporu (PDF)
│   ├── UML_Diyagramlari.pdf         # Use Case ve Sınıf Diyagramları (PDF)
│   ├── ModulerTasarim.pdf           # Modüler Sistem Tasarımı Raporu (PDF)
│   └── SemptomAnaliz-Dokumantasyon.pdf # Genel Proje Raporu (PDF)
│
├── src/                             # Kaynak Kod Klasörü
│   ├── core/                        # Domain Katmanı (Modeller, Soyut Sınıflar, Interfaces)
│   ├── modules/                     # Özellik Modülleri (Ölçeklenebilir yapılar)
│   ├── services/                    # İş Mantığı Katmanı (Hesaplama motorları, servisler)
│   ├── data/                        # Veri Erişim Katmanı (Context, Repositories, Migrations)
│   ├── ui/                          # Sunum Katmanı (Controllers, Views, wwwroot)
│   └── utils/                       # Yardımcı Araçlar ve Fonksiyonlar
│
├── assets/                          # Medya ve Statik Varlıklar
│   ├── images/                      # Görsel kaynaklar
│   ├── sounds/                      # Ses dosyaları
│   └── icons/                       # İkon dosyaları
│
├── data/                            # Örnek Veri Setleri
│   └── data/                        # Örnek CSV, JSON veya veri dosyaları
│
├── tests/                           # Birim Test Katmanı
│   └── SemptomAnalizApp.Tests.csproj # xUnit birim testleri (27 test senaryosu)
│
├── SemptomAnalizApp.sln             # Visual Studio Çözüm Dosyası
└── README.md                        # Çalıştırma ve Kurulum Talimatı
```

*   **Gereksinim Analizi Raporu (PDF):** [docs/GereksinimAnalizi.pdf](docs/GereksinimAnalizi.pdf)
*   **UML Tasarım Diyagramları (PDF):** [docs/UML_Diyagramlari.pdf](docs/UML_Diyagramlari.pdf)
*   **Modüler Sistem Tasarımı Raporu (PDF):** [docs/ModulerTasarim.pdf](docs/ModulerTasarim.pdf)
*   **Genel Proje Raporu (PDF):** [docs/SemptomAnaliz-Dokumantasyon.pdf](docs/SemptomAnaliz-Dokumantasyon.pdf)

---

## OOP Özellikleri ve Hata Yönetimi

| OOP / Tasarım Kavramı | Uygulama Yöntemi |
|-------------|----------|
| **Kalıtım (Inheritance)** | `BaseEntity` soyut sınıfı -> `AnalizOturumu`, `Semptom`, `Hastalik` vb. tüm veri modellerine temel teşkil eder. |
| **Kalıtım (Inheritance)** | `Kullanici` -> ASP.NET Core Identity altyapısındaki `IdentityUser` sınıfından miras alır. |
| **Çok Biçimlilik (Polymorphism)** | `IAnalizService` arayüzü -> Somut `AnalizMotoru` implementasyonu. Kontrolcüler sadece soyut arayüze bağımlıdır. |
| **Çok Biçimlilik (Polymorphism)** | `IGenericRepository<T>` arayüzü -> Somut `GenericRepository<T>` implementasyonu. |
| **Kapsülleme (Encapsulation)** | `AnalizMotoru` yalnızca genel iş akışını yönetir; BMI, Bayes, aciliyet, tekrar ve öneri hesaplama mantığı alt servislerde gizlenmiştir. |
| **Kapsülleme (Encapsulation)** | `GenericRepository<T>` içindeki `DbSet<T>` veri kümesi dış dünyaya kapatılarak `protected readonly DbSet<T>` olarak korunmuştur. |
| **Hata Yönetimi (Robustness)** | Tüm kullanıcı girişleri, DB bağlantıları ve form verileri Controller metotlarında `try-catch` blokları ve ModelState doğrulamalarıyla korunur. |

---

## Mimari

```
Web (Sunum) → Service (İş Mantığı) → Core (Domain)
Web (Sunum) → Data (Veri Erişim) → Core (Domain)
```

> İş mantığı veri erişimi için `IAnalizDbContext` sözleşmesini kullanır; somut EF Core bağlantısı Web katmanında DI ile verilir.  
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
cp src/ui/appsettings.example.json src/ui/appsettings.json
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
dotnet run --project src/ui
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
