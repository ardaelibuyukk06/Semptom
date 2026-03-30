using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Data;
using SemptomAnalizApp.Service.Interfaces;

namespace SemptomAnalizApp.Service.Services;

// ═══════════════════════════════════════════════════════════════════════════
//  ANALİZ MOTORU — Naive Bayes Log-Likelihood Tabanlı Karar Destek Sistemi
//
//  ÖNEMLI UYARI: Bu motor istatistiksel benzerlik skoru üretir.
//  Tıbbi teşhis değildir. Kalibre edilmiş olasılık iddiasında bulunmaz.
//  Her analiz çıktısı "Benzerlik Skoru" olarak sunulur, yüzde olasılık değil.
//
//  Algoritma:
//  log_posterior(D) = log(prior(D)) + Σ log(P(Si|D)/P(Si|¬D)) [seçili]
//                   + Σ 0.4×log(P(Si_absent|D)/P(Si_absent|¬D)) [yüksek ağırlıklı eksik]
//  + log(yaş_modifier × cinsiyet_modifier × süre_modifier × şiddet_modifier)
//
//  Sonuçlar softmax ile normalize edilerek göreli skor (0-100) haline getirilir.
// ═══════════════════════════════════════════════════════════════════════════

public class AnalizMotoru(AppDbContext db) : IAnalizService
{
    // P(Semptom | Hastalık YOK) — genel popülasyonda bir semptomu taşıma olasılığı
    private const double P_S_GIVEN_NOT_D = 0.07;

    public async Task<AnalizSonucu> AnalizEtAsync(
        string kullaniciId,
        IEnumerable<SemptomGirdisi> semptomGirdileri,
        string? ekNotlar)
    {
        var girdiler     = semptomGirdileri.ToList();
        var semptomIdler = girdiler.Select(g => g.SemptomId).ToList(); // SemptomKatalog.Id listesi

        // 1. Sağlık profili
        var profil = await db.SaglikProfilleri
            .FirstOrDefaultAsync(p => p.KullaniciId == kullaniciId);

        // 2. BMI
        var (bmi, bmiKat) = HesaplaBmi(profil);

        // 3. Semptom imzası
        var imza = OlusturImza(semptomIdler);

        // 4. Katalog ID → Motor Semptom ID dönüşümü (FK tabanlı)
        var katalogToMotorMap = await db.SemptomKatalog
            .Where(sk => semptomIdler.Contains(sk.Id) && sk.SemptomId != null)
            .Select(sk => new { KatalogId = sk.Id, MotorId = sk.SemptomId!.Value })
            .ToDictionaryAsync(x => x.KatalogId, x => x.MotorId);

        var motorSemptomIdler = katalogToMotorMap.Values.ToList();
        var motorSemptomSet   = motorSemptomIdler.ToHashSet();

        var motorGirdileri = girdiler
            .Where(g => katalogToMotorMap.ContainsKey(g.SemptomId))
            .Select(g => g with { SemptomId = katalogToMotorMap[g.SemptomId] })
            .ToList();

        // 5. Kritik semptom kontrolü
        var kritikVarMi = motorSemptomIdler.Any() &&
            await db.Semptomlar.AnyAsync(s => s.KritikMi && motorSemptomIdler.Contains(s.Id));
        if (!kritikVarMi)
            kritikVarMi = semptomIdler.Any(id => new[] { 30, 7, 32 }.Contains(id));

        // 6. Bayesian olası durum hesabı
        int kullaniciYas     = profil?.Yas ?? 0;
        string cinsiyet      = profil?.Cinsiyet ?? "";
        double ortSureGun    = girdiler.Any() ? girdiler.Average(g => g.SureGun) : 1.0;

        var olasiDurumlar = await HesaplaBayesianOlasiDurumlarAsync(
            motorSemptomSet, motorGirdileri, kullaniciYas, cinsiyet, ortSureGun, bmiKat);

        // 7. Aciliyet skoru
        var aciliyetSkoru    = HesaplaAciliyetSkoru(girdiler, olasiDurumlar, profil, kritikVarMi);
        var aciliyetSeviyesi = SkoraSeviyeAta(aciliyetSkoru);

        // 8. Tekrar skoru
        var (tekrarSkoru, enYakinGun) = await TekrarTespitEt(kullaniciId, imza);

        // 9. Önerilen bölüm
        var onerilenBolum = await GetOnerilenBolumAsync(olasiDurumlar);

        // 10. Açıklama ve genel yorum
        var aciklama   = OlusturAciklama(girdiler, olasiDurumlar, profil, bmiKat);
        var genelYorum = OlusturGenelYorum(aciliyetSeviyesi, olasiDurumlar, tekrarSkoru, enYakinGun, profil);

        // 11. Günlük öneriler + uyarılar
        var gunlukOneriler = BelirleGunlukOneriler(semptomIdler, aciliyetSeviyesi);
        var uyarilar       = BelirleUyarilar(semptomIdler, aciliyetSeviyesi, kritikVarMi);

        // 12. Tek transaction ile kaydet
        var oturum = new AnalizOturumu
        {
            KullaniciId   = kullaniciId,
            EkNotlar      = ekNotlar,
            SemptomImzasi = imza
        };
        db.AnalizOturumlari.Add(oturum);

        var sonuc = new AnalizSonucu
        {
            AnalizOturumu          = oturum,
            AciliyetSeviyesi       = aciliyetSeviyesi,
            AciliyetSkoru          = aciliyetSkoru,
            OnerilenBolum          = onerilenBolum,
            NedenAciklamasi        = aciklama,
            GenelYorum             = genelYorum,
            TekrarSkoru            = tekrarSkoru,
            EnYakinTekrarGunOncesi = enYakinGun,
            HesaplananBmi          = bmi,
            BmiKategori            = bmiKat,
            GunlukOnerilerJson     = JsonSerializer.Serialize(gunlukOneriler),
            UyariGostergeleriJson  = JsonSerializer.Serialize(uyarilar)
        };
        db.AnalizSonuclari.Add(sonuc);

        foreach (var g in girdiler)
        {
            db.AnalizSemptomlari.Add(new AnalizSemptomu
            {
                AnalizOturumu    = oturum,
                SemptomKatalogId = g.SemptomId,
                Siddet           = (SemptomSiddeti)g.Siddet,
                SureGun          = g.SureGun
            });
        }

        foreach (var d in olasiDurumlar)
        {
            d.AnalizSonucu = sonuc;
            db.OlasiDurumlar.Add(d);
        }

        await db.SaveChangesAsync(); // tek transaction

        return sonuc;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  BAYESIAN LOG-LIKELIHOOD MOTORU
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<List<OlasiDurum>> HesaplaBayesianOlasiDurumlarAsync(
        HashSet<int> motorSemptomSet,
        List<SemptomGirdisi> motorGirdileri,
        int yas,
        string cinsiyet,
        double ortSureGun,
        BmiKategori bmiKat)
    {
        if (!motorSemptomSet.Any()) return [];

        // ADIM 1: Seçilen semptomlarla eşleşen hastalık ID'lerini bul (optimized)
        var eslesen = await db.HastalikSemptomlar
            .Where(hs => motorSemptomSet.Contains(hs.SemptomId))
            .Select(hs => hs.HastalikId)
            .Distinct()
            .ToListAsync();

        if (!eslesen.Any()) return [];

        // ADIM 2: Sadece eşleşen hastalıkların TAM semptom profilini yükle
        var hastalikSemptomlar = await db.HastalikSemptomlar
            .Include(hs => hs.Hastalik)
            .Where(hs => eslesen.Contains(hs.HastalikId))
            .ToListAsync();

        var hastalikGruplari = hastalikSemptomlar.GroupBy(hs => hs.HastalikId);
        var sonuclar = new List<(OlasiDurum durum, double logPosterior)>();

        foreach (var grup in hastalikGruplari)
        {
            var hastalik = grup.First().Hastalik;

            // ── Prior: log(P(D)) ─────────────────────────────────────────
            double logPrior = Math.Log(hastalik.Prevalans + 1.0);

            // ── Log-Likelihood Ratio ─────────────────────────────────────
            double logLLR        = 0;
            int seciliSayi       = 0;

            foreach (var hs in grup)
            {
                // P(S|D): semptomu hastalık varken taşıma olasılığı
                // Ağırlık 1..10 → P 0.13..0.85 (Laplace smoothed linear mapping)
                double pSGivenD = hs.Agirlik / 10.0 * 0.72 + 0.13;

                if (motorSemptomSet.Contains(hs.SemptomId))
                {
                    // Semptom seçildi: pozitif kanıt
                    logLLR += Math.Log(pSGivenD / P_S_GIVEN_NOT_D);
                    seciliSayi++;
                }
                else if (hs.Agirlik >= 7)
                {
                    // Yüksek ağırlıklı semptom SEÇİLMEDİ: negatif kanıt (0.4 ağırlıkla)
                    double pAbsGivenD    = 1.0 - pSGivenD;
                    double pAbsGivenNotD = 1.0 - P_S_GIVEN_NOT_D;
                    logLLR += 0.4 * Math.Log(pAbsGivenD / pAbsGivenNotD);
                }
            }

            if (seciliSayi == 0) continue;

            // ── Yaş Modifier ────────────────────────────────────────────
            double ageMod = 1.0;
            if (yas > 0)
            {
                if (hastalik.MinYas.HasValue && yas < hastalik.MinYas.Value)
                    ageMod *= Math.Max(0.3, 1.0 - (hastalik.MinYas.Value - yas) * 0.025);
                if (hastalik.MaxYas.HasValue && yas > hastalik.MaxYas.Value)
                    ageMod *= Math.Max(0.4, 1.0 - (yas - hastalik.MaxYas.Value) * 0.02);
            }

            // ── Cinsiyet Modifier ────────────────────────────────────────
            // ErkekKatsayi/KadinKatsayi 0 olursa (eski kayıtlar) 1.0 kullan
            double erkekMod = hastalik.ErkekKatsayi > 0 ? (double)hastalik.ErkekKatsayi : 1.0;
            double kadinMod = hastalik.KadinKatsayi > 0 ? (double)hastalik.KadinKatsayi : 1.0;
            double sexMod   = cinsiyet == "Erkek" ? erkekMod
                            : cinsiyet == "Kadın" ? kadinMod
                            : 1.0;

            // ── Süre Modifier ────────────────────────────────────────────
            double durMod = 1.0;
            if (hastalik.AkutMu)
            {
                // Akut hastalık: kısa süre = güçlü sinyal, uzun süre = azalan ihtimal
                durMod = ortSureGun <= 7  ? 1.25
                       : ortSureGun <= 14 ? 1.0
                       : ortSureGun <= 30 ? 0.65
                       : 0.35;
            }
            else
            {
                // Kronik hastalık: uzun süre = daha güçlü sinyal
                durMod = ortSureGun >= 30 ? 1.3
                       : ortSureGun >= 14 ? 1.1
                       : ortSureGun >= 7  ? 0.9
                       : 0.7;
            }

            // ── Şiddet Modifier ──────────────────────────────────────────
            double ortSiddet = motorGirdileri
                .Where(g => grup.Any(hs => hs.SemptomId == g.SemptomId))
                .DefaultIfEmpty()
                .Average(g => g?.Siddet ?? 2.0);
            double siddetMod = 0.80 + ortSiddet * 0.10; // Hafif:0.90, Orta:1.00, Şiddetli:1.10

            // ── BMI Modifier ─────────────────────────────────────────────
            double bmiMod = bmiKat is BmiKategori.ObezeI or BmiKategori.ObezeII ? 1.12 : 1.0;

            // ── Log Posterior ────────────────────────────────────────────
            double logPosterior = logPrior + logLLR
                + Math.Log(Math.Max(1e-10, ageMod * sexMod * durMod * siddetMod * bmiMod));

            sonuclar.Add((new OlasiDurum
            {
                Ad         = hastalik.Ad,
                Aciklama   = hastalik.Aciklama,
                HastalikId = hastalik.Id
            }, logPosterior));
        }

        if (!sonuclar.Any()) return [];

        // ── Softmax Normalizasyonu ────────────────────────────────────────
        // Sayısal stabilite için en büyük değeri çıkar
        double maxLog = sonuclar.Max(x => x.logPosterior);
        var expList   = sonuclar.Select(x => Math.Exp(x.logPosterior - maxLog)).ToList();
        double sumExp = expList.Sum();

        for (int i = 0; i < sonuclar.Count; i++)
        {
            sonuclar[i].durum.SkorYuzdesi = Math.Max(1, (int)Math.Round(expList[i] / sumExp * 100));
        }

        // En iyi 5 sonucu al
        return [.. sonuclar
            .OrderByDescending(x => x.logPosterior)
            .Take(5)
            .Select(x => x.durum)];
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  YARDIMCI METODLAR
    // ═══════════════════════════════════════════════════════════════════════

    private async Task<string> GetOnerilenBolumAsync(List<OlasiDurum> olasiDurumlar)
    {
        if (!olasiDurumlar.Any()) return "Aile Hekimliği";
        var ilk = olasiDurumlar[0];
        if (ilk.HastalikId.HasValue)
        {
            var h = await db.Hastaliklar.FindAsync(ilk.HastalikId.Value);
            return h?.OnerilenBolum ?? "Aile Hekimliği";
        }
        return "Aile Hekimliği";
    }

    internal static (decimal bmi, BmiKategori kat) HesaplaBmi(SaglikProfili? profil)
    {
        if (profil == null || profil.Boy <= 0 || profil.Kilo <= 0)
            return (0, BmiKategori.Normal);
        var boyM = profil.Boy / 100m;
        var bmi  = Math.Round(profil.Kilo / (boyM * boyM), 1);
        var kat  = bmi switch
        {
            < 18.5m => BmiKategori.ZayifAltinda,
            < 25.0m => BmiKategori.Normal,
            < 30.0m => BmiKategori.Fazlakilolu,
            < 35.0m => BmiKategori.ObezeI,
            _       => BmiKategori.ObezeII
        };
        return (bmi, kat);
    }

    internal static string OlusturImza(List<int> semptomIdler)
    {
        var sirali = string.Join("|", semptomIdler.OrderBy(x => x));
        var hash   = SHA256.HashData(Encoding.UTF8.GetBytes(sirali));
        return Convert.ToHexString(hash)[..12].ToLower();
    }

    internal static int HesaplaAciliyetSkoru(
        List<SemptomGirdisi> girdiler,
        List<OlasiDurum> olasiDurumlar,
        SaglikProfili? profil,
        bool kritikVarMi)
    {
        if (!girdiler.Any()) return 10;

        // Temel skor: şiddet × süre etkisi
        double bazSkor = girdiler.Average(g => g.Siddet * Math.Log(g.SureGun + 1) * 12.0);
        bazSkor = Math.Min(bazSkor, 65);

        // En olası durum skoru alt sınır
        if (olasiDurumlar.Count > 0)
            bazSkor = Math.Max(bazSkor, olasiDurumlar[0].SkorYuzdesi * 0.7);

        if (kritikVarMi)
            bazSkor = Math.Min(100, bazSkor + 22);

        if (!string.IsNullOrEmpty(profil?.KronikHastaliklar))
            bazSkor = Math.Min(100, bazSkor + 8);

        if (profil?.Yas >= 65)
            bazSkor = Math.Min(100, bazSkor + 5);

        return (int)Math.Ceiling(bazSkor);
    }

    internal static AciliyetSeviyesi SkoraSeviyeAta(int skor) => skor switch
    {
        <= 25 => AciliyetSeviyesi.Normal,
        <= 50 => AciliyetSeviyesi.Izle,
        <= 75 => AciliyetSeviyesi.Dikkat,
        _     => AciliyetSeviyesi.Acil
    };

    private async Task<(int tekrar, int? enYakinGun)> TekrarTespitEt(string kullaniciId, string imza)
    {
        var sinir = DateTime.UtcNow.AddDays(-30);
        var eskiler = await db.AnalizOturumlari
            .Where(o => o.KullaniciId == kullaniciId
                     && o.SemptomImzasi == imza
                     && o.OlusturulmaTarihi >= sinir)
            .OrderByDescending(o => o.OlusturulmaTarihi)
            .ToListAsync();

        if (!eskiler.Any()) return (0, null);
        var enYakin = (int)(DateTime.UtcNow - eskiler.First().OlusturulmaTarihi).TotalDays;
        return (eskiler.Count, enYakin);
    }

    private static string OlusturAciklama(
        List<SemptomGirdisi> girdiler,
        List<OlasiDurum> olasiDurumlar,
        SaglikProfili? profil,
        BmiKategori bmiKat)
    {
        var sb = new StringBuilder();

        if (olasiDurumlar.Count > 0)
            sb.Append($"{olasiDurumlar[0].Aciklama} ");

        double ortSure   = girdiler.Any() ? girdiler.Average(g => g.SureGun) : 0;
        double ortSiddet = girdiler.Any() ? girdiler.Average(g => g.Siddet)  : 0;

        sb.Append($"Girilen {girdiler.Count} semptomun ortalama {(int)ortSure} gündür sürmesi ve ");
        sb.Append(ortSiddet switch
        {
            < 1.5 => "hafif şiddette seyretmesi",
            < 2.5 => "orta şiddette seyretmesi",
            _     => "yüksek şiddette seyretmesi"
        });
        sb.Append(" benzerlik skorunu belirleyen başlıca faktörler arasındadır.");

        if (girdiler.Any(g => g.Siddet == 3))
        {
            int siddetliSayisi = girdiler.Count(g => g.Siddet == 3);
            sb.Append($" {siddetliSayisi} semptom şiddetli olarak işaretlendiğinden genel aciliyet skoru daha yüksek hesaplanmıştır.");
        }

        if (girdiler.Any(g => g.SureGun >= 14))
            sb.Append(" İki haftayı aşan semptomlar kronik hastalık ilişkisinin araştırılmasını önemli kılar.");

        if (bmiKat is BmiKategori.ObezeI or BmiKategori.ObezeII)
            sb.Append(" Yüksek VKİ değeri bazı durumların görülme sıklığını artırmaktadır.");

        if (!string.IsNullOrEmpty(profil?.KronikHastaliklar))
            sb.Append($" Kayıtlı kronik hastalıklar ({profil.KronikHastaliklar}) değerlendirmeye dahil edilmiştir.");

        if (profil?.Yas >= 65)
            sb.Append(" İleri yaş grubu için aciliyet eşiği daha düşük tutulmuştur.");

        sb.Append(" Analiz yalnızca sisteme girilmiş bilgilere dayanmaktadır; muayene, laboratuvar ve görüntüleme bulguları dahil değildir.");

        return sb.ToString();
    }

    private static string OlusturGenelYorum(
        AciliyetSeviyesi seviye,
        List<OlasiDurum> olasiDurumlar,
        int tekrar,
        int? enYakinGun,
        SaglikProfili? profil)
    {
        var sb = new StringBuilder();

        sb.Append(seviye switch
        {
            AciliyetSeviyesi.Normal  => "Genel tablonuz normal sınırlar içinde görünmektedir. ",
            AciliyetSeviyesi.Izle    => "Belirtileriniz hafif–orta düzeyde seyrediyor; durumu takip etmeniz önerilir. ",
            AciliyetSeviyesi.Dikkat  => "Semptomlarınız dikkat gerektiren bir tablo oluşturmaktadır; yakın zamanda bir sağlık profesyoneliyle görüşmeniz tavsiye edilir. ",
            AciliyetSeviyesi.Acil    => "Mevcut bulgular acil tıbbi değerlendirme gerektirebilir; lütfen en kısa sürede bir sağlık kuruluşuna başvurun. ",
            _                        => string.Empty
        });

        if (olasiDurumlar.Count > 0)
            sb.Append($"İstatistiksel benzerlik analizi en yüksek uyumu '{olasiDurumlar[0].Ad}' tablosuyla göstermektedir. ");

        if (tekrar > 0 && enYakinGun.HasValue)
            sb.Append($"Bu semptom kombinasyonu son 30 gün içinde {tekrar} kez kaydedilmiştir; {enYakinGun} gün önce benzer bir tablo mevcut. Tekrarlayan belirtiler altta yatan bir durumu düşündürebilir. ");

        if (profil != null && !string.IsNullOrEmpty(profil.KronikHastaliklar))
            sb.Append("Kronik hastalık geçmişiniz nedeniyle bulgularınızın bir uzman tarafından değerlendirilmesi özellikle önem taşımaktadır.");

        sb.Append(" ⚠ Bu değerlendirme klinik teşhis veya tıbbi tavsiye değildir; yalnızca istatistiksel benzerlik skoruna dayanan bir karar destek çıktısıdır. Kesin değerlendirme için bir sağlık profesyoneline başvurunuz.");

        return sb.ToString();
    }

    private static List<GunlukOneriDto> BelirleGunlukOneriler(
        List<int> semptomIdler, AciliyetSeviyesi seviye)
    {
        var oneriler = new List<GunlukOneriDto>
        {
            new("💧", "Sıvı Tüketimi",  "Günde en az 2–3 litre su için."),
            new("🛌", "Yeterli Dinlenme", "Düzenli ve yeterli uyku iyileşmeyi destekler."),
        };

        if (semptomIdler.Contains(21))
            oneriler.Add(new("🌡️", "Ateş Takibi", "Sabah ve akşam ateşinizi ölçüp not alın."));

        if (semptomIdler.Contains(6))
            oneriler.Add(new("🫖", "Sıcak İçecek", "Ihlamur veya zencefilli içecekler gırtlağı rahatlatabilir."));

        if (semptomIdler.Intersect([16, 17, 18, 19]).Any())
            oneriler.Add(new("🧘", "Hafif Hareket", "Ağır aktiviteden kaçının; hafif germe hareketleri yapabilirsiniz."));

        if (semptomIdler.Intersect([11, 12, 13, 14]).Any())
            oneriler.Add(new("🥗", "Hafif Beslenme", "Yağlı ve ağır yiyeceklerden uzak durarak bağırsak sistemini dinlendirin."));

        if (semptomIdler.Intersect([30, 7, 31, 32]).Any())
            oneriler.Add(new("🏥", "Acil Değerlendirme", "Bu semptomlar için vakit kaybetmeden bir sağlık kuruluşuna başvurun."));
        else if (seviye is AciliyetSeviyesi.Normal or AciliyetSeviyesi.Izle)
            oneriler.Add(new("🚶", "Kısa Yürüyüş", "Belirtileriniz hafifse kısa süreli yürüyüş faydalı olabilir."));

        return oneriler;
    }

    private static List<string> BelirleUyarilar(
        List<int> semptomIdler, AciliyetSeviyesi seviye, bool kritikVarMi)
    {
        var uyarilar = new List<string>
        {
            "Belirtileriniz 5 günden uzun sürer veya kötüleşirse bir doktora başvurun.",
            "Ateş 39°C üzerine çıkarsa vakit kaybetmeden sağlık kuruluşuna gidin.",
        };

        if (kritikVarMi)
            uyarilar.Insert(0, "⚠️ Göğüs ağrısı veya nefes darlığı kötüleşirse ACİL servisi arayın.");

        if (semptomIdler.Contains(3))
            uyarilar.Add("Ani gelişen şiddetli baş dönmesi veya görme bozukluğu acil değerlendirme gerektirir.");

        if (semptomIdler.Contains(13))
            uyarilar.Add("Kusma 24 saatten uzun sürerse veya kanlı kusma olursa acilen başvurun.");

        if (semptomIdler.Contains(26))
            uyarilar.Add("İstem dışı kilo kaybı devam ediyorsa mutlaka doktor kontrolü yaptırın.");

        if (seviye == AciliyetSeviyesi.Acil)
            uyarilar.Insert(0, "🔴 Aciliyet düzeyi yüksek — lütfen en kısa sürede bir sağlık profesyoneline görünün.");

        return uyarilar;
    }
}

public record GunlukOneriDto(string Ikon, string Baslik, string Metin);
