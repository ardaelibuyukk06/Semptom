using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Data;
using SemptomAnalizApp.Web.ViewModels;

namespace SemptomAnalizApp.Web.Controllers;

[Authorize]
public class GecmisController(AppDbContext db, UserManager<Kullanici> userManager) : Controller
{
    private const int MaksimumGecmisKaydi = 120;

    private static readonly string[] AyAdlari =
        ["Ocak","Şubat","Mart","Nisan","Mayıs","Haziran",
         "Temmuz","Ağustos","Eylül","Ekim","Kasım","Aralık"];

    public async Task<IActionResult> Index()
    {
        var kullanici = await userManager.GetUserAsync(User);

        var oturumlar = await db.AnalizOturumlari
            .Include(o => o.AnalizSonucu)
            .Include(o => o.AnalizSemptomlari)
                .ThenInclude(s => s.SemptomKatalog)
            .Where(o => o.KullaniciId == kullanici!.Id)
            .OrderByDescending(o => o.OlusturulmaTarihi)
            .Take(MaksimumGecmisKaydi)
            .ToListAsync();

        var yilGruplari = oturumlar
            .GroupBy(o => o.OlusturulmaTarihi.Year)
            .OrderByDescending(g => g.Key)
            .Select(yilG => new GecmisYilGrubu
            {
                Yil = yilG.Key,
                AyGruplari = yilG
                    .GroupBy(o => o.OlusturulmaTarihi.Month)
                    .OrderByDescending(g => g.Key)
                    .Select(ayG => new GecmisAyGrubu
                    {
                        AyAdi = AyAdlari[ayG.Key - 1],
                        Kayitlar = ayG.Select(o =>
                        {
                            string etiket = "Normal", renk = "success";
                            int skor = 0;
                            if (o.AnalizSonucu != null)
                            {
                                skor = o.AnalizSonucu.AciliyetSkoru;
                                (etiket, renk) = o.AnalizSonucu.AciliyetSeviyesi switch
                                {
                                    AciliyetSeviyesi.Acil    => ("Acil", "danger"),
                                    AciliyetSeviyesi.Dikkat  => ("Dikkat", "warning"),
                                    AciliyetSeviyesi.Izle    => ("İzle", "info"),
                                    _ => ("Normal", "success")
                                };
                            }

                            return new GecmisKayit
                            {
                                OturumId = o.Id,
                                SonucId = o.AnalizSonucu?.Id ?? 0,
                                Tarih = o.OlusturulmaTarihi,
                                Semptomlar = o.AnalizSemptomlari
                                    .Select(s => s.SemptomKatalog?.Ad ?? "")
                                    .ToList(),
                                AciliyetEtiketi = etiket,
                                AciliyetRengi = renk,
                                AciliyetSkoru = skor,
                                OnerilenBolum = o.AnalizSonucu?.OnerilenBolum ?? "",
                                SemptomImzasi = o.SemptomImzasi
                            };
                        }).ToList()
                    }).ToList()
            }).ToList();

        return View(new GecmisViewModel { YilGruplari = yilGruplari });
    }
}
