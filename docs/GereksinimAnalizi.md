# Gereksinim Analizi Dokümanı

**Proje Adı:** SemptomAnaliz — Bireysel Sağlık Farkındalık Uygulaması  
**Hazırlayanlar:** Bilgisayar Programcılığı Proje Ekibi — Arda Elibüyük, Furkan Taşdelen  
**Tarih:** Nisan 2026  
**Versiyon:** 1.0

---

## 1. Proje Amacı ve Kapsamı

SemptomAnaliz; kullanıcıların hissettikleri semptomları sisteme girerek,
Naive Bayes log-likelihood algoritması aracılığıyla olası hastalık benzerlikleri
hakkında istatistiksel bilgi edinmesini sağlayan bir web uygulamasıdır.

> **Önemli Uyarı:** Bu uygulama tıbbi teşhis niteliği taşımaz.
> Tüm sonuçlar istatistiksel benzerlik analizidir. Kesin tanı için
> mutlaka doktor/uzman görüşü alınmalıdır.

---

## 2. Paydaş Analizi

| Paydaş | Rol | Beklenti |
|--------|-----|---------|
| Bireysel Kullanıcı | Son kullanıcı | Semptomlarını girerek olası durumları öğrenmek |
| Admin | Sistem yöneticisi | Kullanıcı ve sistem verilerini yönetmek |
| Geliştirici Ekibi | Teknik | Bakımı kolay, güvenli, genişletilebilir sistem |

---

## 3. Fonksiyonel Gereksinimler

### 3.1 Kullanıcı Yönetimi

| Kod | Gereksinim | Öncelik |
|-----|-----------|---------|
| KY-01 | Kullanıcı e-posta ve şifre ile kayıt olabilmelidir | Yüksek |
| KY-02 | Kayıt sırasında KVKK ve tıbbi sorumluluk reddi onayı alınmalıdır | Yüksek |
| KY-03 | Kayıtlı kullanıcı sisteme giriş yapabilmelidir | Yüksek |
| KY-04 | Kullanıcı "beni hatırla" seçeneği ile 14 gün oturum açık tutabilmelidir | Orta |
| KY-05 | Kullanıcı sistemden çıkış yapabilmelidir | Yüksek |
| KY-06 | Admin rolündeki kullanıcı tüm kullanıcı listesini görebilmelidir | Yüksek |

### 3.2 Sağlık Profili

| Kod | Gereksinim | Öncelik |
|-----|-----------|---------|
| SP-01 | Kullanıcı boy, kilo, yaş, cinsiyet bilgilerini girebilmelidir | Yüksek |
| SP-02 | Kullanıcı kronik hastalıklarını belirtebilmelidir | Orta |
| SP-03 | Sistem, girilen boy/kilo değerlerine göre BMI hesaplamalıdır | Yüksek |
| SP-04 | BMI kategorisi görsel olarak gösterilmelidir | Orta |
| SP-05 | Profil bilgileri herhangi bir zamanda güncellenebilmelidir | Yüksek |

### 3.3 Semptom Analizi

| Kod | Gereksinim | Öncelik |
|-----|-----------|---------|
| SA-01 | Kullanıcı kategori bazlı semptom listesinden seçim yapabilmelidir | Yüksek |
| SA-02 | Her semptom için şiddet (1-3) ve süre (gün) girilebilmelidir | Yüksek |
| SA-03 | Ek not alanı bulunmalıdır | Düşük |
| SA-04 | Sistem Naive Bayes algoritması ile olası durumları hesaplamalıdır | Yüksek |
| SA-05 | Aciliyet skoru (0–100) ve seviyesi gösterilmelidir | Yüksek |
| SA-06 | Kritik semptomlar (göğüs ağrısı, nefes darlığı vb.) için acil uyarı tetiklenmelidir | Yüksek |
| SA-07 | Olası durumlar göreli benzerlik skoruyla sıralanmalıdır | Yüksek |
| SA-08 | Günlük öneriler ve uyarı göstergeleri sunulmalıdır | Orta |
| SA-09 | Radar grafiği ile semptom dağılımı görselleştirilmelidir | Orta |
| SA-10 | Aynı semptom imzası daha önce analiz edilmişse tekrar skoru gösterilmelidir | Orta |

### 3.4 Analiz Geçmişi

| Kod | Gereksinim | Öncelik |
|-----|-----------|---------|
| AG-01 | Kullanıcı geçmiş analizlerini listeleyebilmelidir | Yüksek |
| AG-02 | Geçmiş analize tıklayarak detay görüntülenebilmelidir | Yüksek |
| AG-03 | Geçmiş liste tarihe göre azalan sırada olmalıdır | Orta |

### 3.5 Admin Paneli

| Kod | Gereksinim | Öncelik |
|-----|-----------|---------|
| AP-01 | Admin tüm kayıtlı kullanıcıları listeleyebilmelidir | Yüksek |
| AP-02 | Admin toplam analiz sayısını görebilmelidir | Orta |
| AP-03 | Admin kullanıcı rollerini görebilmelidir | Orta |

---

## 4. Fonksiyonel Olmayan Gereksinimler

| Kod | Gereksinim | Detay |
|-----|-----------|-------|
| FO-01 | Güvenlik | HTTPS/HSTS, CSRF token, X-Frame-Options, X-Content-Type-Options güvenlik başlıkları |
| FO-02 | Performans | Analiz süresi < 3 saniye |
| FO-03 | Rate Limiting | Dakikada en fazla 10 analiz isteği (kötüye kullanım önleme) |
| FO-04 | Şifre güvenliği | Minimum 8 karakter, büyük/küçük harf, rakam ve sembol zorunlu; bcrypt hash ile saklanır |
| FO-05 | KVKK uyumu | Kullanıcı verisi açık rıza ile toplanır, kayıt formunda onay zorunludur |
| FO-06 | Kullanılabilirlik | Mobil uyumlu (responsive) Bootstrap 5 arayüzü |
| FO-07 | Ölçeklenebilirlik | PostgreSQL connection pooling ile eşzamanlı istekler desteklenir |

---

## 5. Sistem Sınırları

- Uygulama tıbbi teşhis üretmez; istatistiksel benzerlik skoru üretir.
- Sistemde 42+ hastalık ve 40+ semptom tanımlıdır (DbSeeder ile otomatik yüklenir).
- PostgreSQL veritabanı zorunludur (Supabase ücretsiz tier ile test edilmiştir).
- Her kullanıcının analiz geçmişi ve sağlık profili yalnızca kendine özeldir.

---

## 6. Kullanım Senaryoları (Özet)

1. **Kayıt ve Giriş:** Kullanıcı sisteme kayıt olur, KVKK onayı verir, giriş yapar.
2. **Profil Oluşturma:** Boy, kilo, yaş, cinsiyet bilgilerini doldurur; BMI otomatik hesaplanır.
3. **Semptom Analizi:** Kategori bazlı listeden semptomları seçer, şiddet/süre girer, analizi başlatır.
4. **Sonuç İnceleme:** Aciliyet skoru, olası durumlar, günlük öneriler ve uyarı sinyalleri gösterilir.
5. **Geçmiş Takip:** Önceki analizler listelenir, tekrar görülen semptom kalıpları uyarı üretir.
6. **Admin Yönetimi:** Admin tüm kullanıcıları ve sistem istatistiklerini görüntüler.

---

## 7. Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| Web Çatısı | ASP.NET Core 9 MVC |
| ORM | Entity Framework Core 9 + Npgsql |
| Veritabanı | PostgreSQL (Supabase) |
| Kimlik Doğrulama | ASP.NET Core Identity (bcrypt, cookie tabanlı) |
| Ön Yüz | Bootstrap 5 dark tema + Chart.js 4 + SweetAlert2 |
| Test | xUnit (27 birim testi) |

---

*Bu doküman SemptomAnaliz projesinin gereksinim analizini kapsamaktadır.*  
*Son güncelleme: Nisan 2026*
