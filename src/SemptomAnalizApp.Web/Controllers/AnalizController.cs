using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Data;
using SemptomAnalizApp.Service.Interfaces;
using SemptomAnalizApp.Web.ViewModels;

namespace SemptomAnalizApp.Web.Controllers;

[Authorize]
public class AnalizController(
    AppDbContext db,
    UserManager<Kullanici> userManager,
    IAnalizService analizService,
    ILogger<AnalizController> logger) : Controller
{
    private const int MaxSemptomSayisi = 30;
    private const int MaxSureGun = 365;

    private static readonly Dictionary<SemptomKategorisi, (string Ad, string Ikon)> KategoriMeta = new()
    {
        [SemptomKategorisi.BasBoyun]  = ("Baş / Boyun", "bi-emoji-dizzy"),
        [SemptomKategorisi.Solunum]   = ("Solunum", "bi-lungs"),
        [SemptomKategorisi.Sindirim]  = ("Sindirim", "bi-droplet-half"),
        [SemptomKategorisi.MasEklem]  = ("Kas / Eklem", "bi-lightning"),
        [SemptomKategorisi.Genel]     = ("Genel", "bi-thermometer-half"),
        [SemptomKategorisi.Deri]      = ("Deri", "bi-hand-index"),
        [SemptomKategorisi.Kalp]      = ("Kalp / Göğüs", "bi-heart-pulse"),
        [SemptomKategorisi.Ruhsal]    = ("Ruh Sağlığı", "bi-brain"),
    };

    [HttpGet]
    public async Task<IActionResult> Yeni()
    {
        var semptomlar = await db.SemptomKatalog
            .Where(s => s.Aktif)
            .OrderBy(s => s.Kategori).ThenBy(s => s.Ad)
            .ToListAsync();

        var gruplari = semptomlar
            .GroupBy(s => s.Kategori)
            .Select(g =>
            {
                var meta = KategoriMeta.GetValueOrDefault(g.Key, (g.Key.ToString(), "bi-circle"));
                return new SemptomGrubu
                {
                    KategoriAdi = meta.Item1,
                    IkonKodu = meta.Item2,
                    Semptomlar = g.Select(s => new SemptomSecenegi
                    {
                        Id = s.Id,
                        Ad = s.Ad,
                        IkonKodu = s.IkonKodu
                    }).ToList()
                };
            }).ToList();

        return View(new YeniAnalizViewModel { SemptomGruplari = gruplari });
    }

    [HttpPost]
    [EnableRateLimiting("analiz")]
    public async Task<IActionResult> Yeni(
        [FromForm] string? semptomIdler,
        [FromForm] string? siddetler,
        [FromForm] string? sureler,
        [FromForm] string? ekNotlar)
    {
        try
        {
            var kullanici = await userManager.GetUserAsync(User);
            if (kullanici == null) return RedirectToAction("Giris", "Hesap");

            if (!TryParseIntCsv(semptomIdler, 1, int.MaxValue, MaxSemptomSayisi, out var idList) ||
                !TryParseIntCsv(siddetler, 1, 3, MaxSemptomSayisi, out var sidList) ||
                !TryParseIntCsv(sureler, 1, MaxSureGun, MaxSemptomSayisi, out var surList))
            {
                TempData["Hata"] = "Semptom bilgileri geçersiz. Lütfen seçimlerinizi kontrol edip tekrar deneyiniz.";
                return RedirectToAction("Yeni");
            }

            if (idList.Count == 0)
            {
                TempData["Hata"] = "Lütfen en az bir semptom seçiniz.";
                return RedirectToAction("Yeni");
            }

            var benzersizIdler = idList.Distinct().ToList();
            var aktifSemptomIdleri = await db.SemptomKatalog
                .Where(s => benzersizIdler.Contains(s.Id) && s.Aktif)
                .Select(s => s.Id)
                .ToListAsync();

            if (aktifSemptomIdleri.Count != benzersizIdler.Count)
            {
                TempData["Hata"] = "Seçilen semptomlardan biri geçersiz veya pasif durumda.";
                return RedirectToAction("Yeni");
            }

            var aktifSemptomSet = aktifSemptomIdleri.ToHashSet();
            var eklenenIdler = new HashSet<int>();
            var girdiler = new List<SemptomGirdisi>();

            for (var i = 0; i < idList.Count; i++)
            {
                var id = idList[i];
                if (!aktifSemptomSet.Contains(id) || !eklenenIdler.Add(id)) continue;

                girdiler.Add(new SemptomGirdisi(
                    id,
                    i < sidList.Count ? sidList[i] : 2,
                    i < surList.Count ? surList[i] : 1));
            }

            var sonuc = await analizService.AnalizEtAsync(kullanici.Id, girdiler, ekNotlar);
            return RedirectToAction("Sonuc", new { id = sonuc.Id });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Analiz oluşturulurken hata oluştu.");
            TempData["Hata"] = "Analiz sırasında beklenmeyen bir hata oluştu. Lütfen tekrar deneyiniz.";
            return RedirectToAction("Yeni");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Sonuc(int id)
    {
        var sonuc = await db.AnalizSonuclari
            .Include(s => s.OlasiDurumlar)
            .Include(s => s.AnalizOturumu)
                .ThenInclude(o => o.AnalizSemptomlari)
                .ThenInclude(s => s.SemptomKatalog)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sonuc == null) return NotFound();

        // Yetki kontrolü
        var kullanici = await userManager.GetUserAsync(User);
        if (sonuc.AnalizOturumu.KullaniciId != kullanici!.Id && !User.IsInRole("Admin"))
            return Forbid();

        var (etiket, renk, arkaPlan) = sonuc.AciliyetSeviyesi switch
        {
            AciliyetSeviyesi.Normal  => ("Normal İzlem", "success", "#d1fae5"),
            AciliyetSeviyesi.Izle    => ("Takip Önerilir", "warning", "#fef9c3"),
            AciliyetSeviyesi.Dikkat  => ("Dikkat Gerekli", "orange", "#ffedd5"),
            AciliyetSeviyesi.Acil    => ("Acil Değerlendirme", "danger", "#fee2e2"),
            _ => ("Normal", "success", "#d1fae5")
        };

        var (bmiMetni, bmiRengi) = sonuc.BmiKategori switch
        {
            BmiKategori.ZayifAltinda => ("Zayıf", "info"),
            BmiKategori.Normal       => ("Normal", "success"),
            BmiKategori.Fazlakilolu  => ("Fazla Kilolu", "warning"),
            BmiKategori.ObezeI       => ("Obez (Sınıf I)", "danger"),
            BmiKategori.ObezeII      => ("Obez (Sınıf II+)", "danger"),
            _ => ("Bilinmiyor", "secondary")
        };

        // BMI yüzdesi (progress bar için, 15–45 arasını temsil eder)
        int bmiYuzdesi = sonuc.HesaplananBmi > 0
            ? (int)Math.Min(100, Math.Max(0, (double)(sonuc.HesaplananBmi - 15) / 30 * 100))
            : 0;

        var gunlukOneriler = JsonSerializer.Deserialize<List<GunlukOneriDto>>(
            sonuc.GunlukOnerilerJson) ?? [];

        var uyarilar = JsonSerializer.Deserialize<List<string>>(
            sonuc.UyariGostergeleriJson) ?? [];

        // Göreli bar genişliği: en yüksek skoru 100 kabul et
        var sirali = sonuc.OlasiDurumlar.OrderByDescending(d => d.SkorYuzdesi).ToList();
        int maxSkor = sirali.Count > 0 ? sirali[0].SkorYuzdesi : 1;

        var olasiDurumlar = sirali.Select(d =>
        {
            int goreli = maxSkor > 0 ? (int)Math.Round(d.SkorYuzdesi * 100.0 / maxSkor) : 0;
            (string etiket, string etiketRenk) = goreli >= 75 ? ("Yüksek Uyum",  "danger")
                                              : goreli >= 45 ? ("Orta Uyum",    "warning")
                                              :                ("Düşük Uyum",   "secondary");
            return new OlasiDurumSatiri
            {
                Ad               = d.Ad,
                SkorYuzdesi      = goreli,
                Aciklama         = d.Aciklama,
                BarRengi         = etiketRenk,
                BenzerlikEtiketi = etiket,
                EtiketRengi      = etiketRenk
            };
        }).ToList();

        // Semptom fingerprint: tüm 8 kategori sabit sırada, her eksende o kategoriden seçilen semptom sayısı
        var radarKategoriler = new (SemptomKategorisi Kat, string Ad)[]
        {
            (SemptomKategorisi.BasBoyun, "Baş/Boyun"),
            (SemptomKategorisi.Solunum,  "Solunum"),
            (SemptomKategorisi.Sindirim, "Sindirim"),
            (SemptomKategorisi.MasEklem, "Kas/Eklem"),
            (SemptomKategorisi.Genel,    "Genel"),
            (SemptomKategorisi.Deri,     "Deri"),
            (SemptomKategorisi.Kalp,     "Kalp/Göğüs"),
            (SemptomKategorisi.Ruhsal,   "Ruh Sağlığı"),
        };
        var radarEtiketler = radarKategoriler.Select(k => k.Ad).ToList();
        var radarVeriler   = radarKategoriler
            .Select(k => sonuc.AnalizOturumu.AnalizSemptomlari
                .Count(s => s.SemptomKatalog?.Kategori == k.Kat))
            .ToList();

        var model = new AnalizSonucViewModel
        {
            OturumId = sonuc.AnalizOturumuId,
            Tarih = sonuc.OlusturulmaTarihi,
            AciliyetSkoru = sonuc.AciliyetSkoru,
            AciliyetEtiketi = etiket,
            AciliyetRengi = renk,
            AciliyetArkaPlan = arkaPlan,
            OnerilenBolum = sonuc.OnerilenBolum,
            NedenAciklamasi = sonuc.NedenAciklamasi,
            GenelYorum = sonuc.GenelYorum,
            SemptomImzasi = sonuc.AnalizOturumu.SemptomImzasi,
            TekrarSkoru = sonuc.TekrarSkoru,
            EnYakinTekrarGunOncesi = sonuc.EnYakinTekrarGunOncesi,
            Bmi = sonuc.HesaplananBmi,
            BmiKategoriMetni = bmiMetni,
            BmiRengi = bmiRengi,
            BmiYuzdesi = bmiYuzdesi,
            OlasiDurumlar = olasiDurumlar,
            GunlukOneriler = gunlukOneriler.Select(o => new GunlukOneriItem
            {
                Ikon = o.Ikon, Baslik = o.Baslik, Metin = o.Metin
            }).ToList(),
            UyariGostergeleri = uyarilar,
            SecilmisSemptomlar = sonuc.AnalizOturumu.AnalizSemptomlari
                .Select(s => s.SemptomKatalog?.Ad ?? "")
                .ToList(),
            RadarEtiketler = radarEtiketler,
            RadarVeriler   = radarVeriler,
        };

        return View(model);
    }

    private static bool TryParseIntCsv(string? raw, int min, int max, int maxCount, out List<int> values)
    {
        values = [];
        if (string.IsNullOrWhiteSpace(raw)) return true;

        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (values.Count >= maxCount) return false;
            if (!int.TryParse(part, out var value)) return false;
            if (value < min || value > max) return false;

            values.Add(value);
        }

        return true;
    }
}
