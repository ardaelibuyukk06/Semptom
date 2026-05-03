using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Data;
using SemptomAnalizApp.Web.ViewModels;

namespace SemptomAnalizApp.Web.Controllers;

public class HomeController(AppDbContext db, UserManager<Kullanici> userManager) : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Dashboard");
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var kullanici = await userManager.GetUserAsync(User);
        if (kullanici == null) return RedirectToAction("Giris", "Hesap");

        var profil = await db.SaglikProfilleri
            .FirstOrDefaultAsync(p => p.KullaniciId == kullanici.Id);

        var toplamAnalizSayisi = await db.AnalizOturumlari
            .Where(o => o.KullaniciId == kullanici.Id)
            .CountAsync();

        var sonAnalizler = await db.AnalizOturumlari
            .Include(o => o.AnalizSonucu)
            .Include(o => o.AnalizSemptomlari)
                .ThenInclude(s => s.SemptomKatalog)
            .Where(o => o.KullaniciId == kullanici.Id)
            .OrderByDescending(o => o.OlusturulmaTarihi)
            .Take(5)
            .ToListAsync();

        var son30Gun = DateTime.UtcNow.AddDays(-30);
        var tekrarlayan = await db.AnalizOturumlari
            .Where(o => o.KullaniciId == kullanici.Id && o.OlusturulmaTarihi >= son30Gun)
            .GroupBy(o => o.SemptomImzasi)
            .Where(g => g.Count() > 1)
            .CountAsync();

        var sonOturum = sonAnalizler.FirstOrDefault();
        string riskOzeti = "Normal";
        string riskRengi = "success";
        if (sonOturum?.AnalizSonucu != null)
        {
            (riskOzeti, riskRengi) = sonOturum.AnalizSonucu.AciliyetSeviyesi switch
            {
                AciliyetSeviyesi.Acil => ("Acil", "danger"),
                AciliyetSeviyesi.Dikkat => ("Dikkat", "warning"),
                AciliyetSeviyesi.Izle => ("İzlemede", "info"),
                _ => ("Normal", "success")
            };
        }

        // Trend verisi: sonucu olan son 10 analiz, kronolojik sıraya çevrilmiş
        var trendOturumlar = await db.AnalizOturumlari
            .Include(o => o.AnalizSonucu)
            .Where(o => o.KullaniciId == kullanici.Id && o.AnalizSonucu != null)
            .OrderByDescending(o => o.OlusturulmaTarihi)
            .Take(10)
            .ToListAsync();

        trendOturumlar = trendOturumlar
            .Where(o => o.AnalizSonucu != null)
            .Reverse()
            .ToList();

        var model = new DashboardViewModel
        {
            KullaniciAd = kullanici.Ad,
            ToplamAnalizSayisi = toplamAnalizSayisi,
            SonAnalizTarihi = sonOturum?.OlusturulmaTarihi,
            TekrarlayaniSemptomSayisi = tekrarlayan,
            RiskOzeti = riskOzeti,
            RiskRengi = riskRengi,
            ProfilTamamlandi = profil != null,
            TrendSkorlar = trendOturumlar
                .Select(o => o.AnalizSonucu!.AciliyetSkoru)
                .ToList(),
            TrendEtiketler = trendOturumlar
                .Select(o => o.OlusturulmaTarihi.ToLocalTime().ToString("dd MMM"))
                .ToList(),
            SonAnalizler = sonAnalizler.Select(o =>
            {
                var semptomAdlari = o.AnalizSemptomlari
                    .Select(s => s.SemptomKatalog?.Ad ?? "")
                    .Take(3)
                    .ToList();

                string etiket = "Normal", renk = "success";
                int skor = 0;
                if (o.AnalizSonucu != null)
                {
                    skor = o.AnalizSonucu.AciliyetSkoru;
                    (etiket, renk) = o.AnalizSonucu.AciliyetSeviyesi switch
                    {
                        AciliyetSeviyesi.Acil => ("Acil", "danger"),
                        AciliyetSeviyesi.Dikkat => ("Dikkat", "warning"),
                        AciliyetSeviyesi.Izle => ("İzle", "info"),
                        _ => ("Normal", "success")
                    };
                }

                return new SonAnalizSatiri
                {
                    Id = o.AnalizSonucu?.Id ?? 0,
                    Tarih = o.OlusturulmaTarihi,
                    AnaSemptomlar = string.Join(", ", semptomAdlari),
                    AciliyetEtiketi = etiket,
                    AciliyetRengi = renk,
                    AciliyetSkoru = skor
                };
            }).ToList()
        };

        return View(model);
    }
}
