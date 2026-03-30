using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Data;
using SemptomAnalizApp.Web.ViewModels;

namespace SemptomAnalizApp.Web.Controllers;

[Authorize]
public class ProfilController(AppDbContext db, UserManager<Kullanici> userManager) : Controller
{
    private static readonly List<string> KronikHastalikListesi =
    [
        "Diyabet", "Hipertansiyon", "Astım", "KOAH", "Kalp Hastalığı",
        "Tiroid Bozukluğu", "Böbrek Hastalığı", "Karaciğer Hastalığı",
        "Romatolojik Hastalık", "Kanser (geçmiş/aktif)", "Epilepsi", "Depresyon/Anksiyete"
    ];

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var kullanici = await userManager.GetUserAsync(User);
        var profil = await db.SaglikProfilleri
            .FirstOrDefaultAsync(p => p.KullaniciId == kullanici!.Id);

        var model = new ProfilViewModel();
        if (profil != null)
        {
            model.Yas = profil.Yas;
            model.Cinsiyet = profil.Cinsiyet;
            model.Boy = profil.Boy;
            model.Kilo = profil.Kilo;
            model.Notlar = profil.Notlar;
            model.SeciliKronikHastaliklar = profil.KronikHastaliklar?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).ToList() ?? [];

            if (profil.Boy > 0 && profil.Kilo > 0)
            {
                var boyM = profil.Boy / 100m;
                model.HesaplananBmi = Math.Round(profil.Kilo / (boyM * boyM), 1);
                (model.BmiKategoriMetni, model.BmiRengi) = BmiMetni(model.HesaplananBmi.Value);
            }
        }

        ViewBag.KronikHastaliklar = KronikHastalikListesi;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProfilViewModel model)
    {
        ViewBag.KronikHastaliklar = KronikHastalikListesi;
        if (!ModelState.IsValid) return View(model);

        var kullanici = await userManager.GetUserAsync(User);
        var profil = await db.SaglikProfilleri
            .FirstOrDefaultAsync(p => p.KullaniciId == kullanici!.Id);

        if (profil == null)
        {
            profil = new SaglikProfili { KullaniciId = kullanici!.Id };
            db.SaglikProfilleri.Add(profil);
        }

        profil.Yas = model.Yas;
        profil.Cinsiyet = model.Cinsiyet;
        profil.Boy = model.Boy;
        profil.Kilo = model.Kilo;
        profil.Notlar = model.Notlar;
        profil.KronikHastaliklar = model.SeciliKronikHastaliklar.Any()
            ? string.Join(", ", model.SeciliKronikHastaliklar)
            : null;
        profil.GuncellenmeTarihi = DateTime.UtcNow;

        await db.SaveChangesAsync();

        TempData["Basarili"] = "Sağlık profiliniz başarıyla kaydedildi.";
        return RedirectToAction("Index");
    }

    private static (string metin, string renk) BmiMetni(decimal bmi) => bmi switch
    {
        < 18.5m => ("Zayıf", "info"),
        < 25.0m => ("Normal", "success"),
        < 30.0m => ("Fazla Kilolu", "warning"),
        < 35.0m => ("Obez (Sınıf I)", "danger"),
        _ => ("Obez (Sınıf II+)", "danger")
    };
}
