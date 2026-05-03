using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Data;

namespace SemptomAnalizApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    AppDbContext db,
    UserManager<Kullanici> userManager,
    ILogger<AdminController> logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        var toplamKullanici = await userManager.Users.CountAsync();
        var toplamAnaliz = await db.AnalizOturumlari.CountAsync();
        var bugunGiris = await userManager.Users
            .Where(u => u.KayitTarihi >= DateTime.UtcNow.AddHours(-24))
            .CountAsync();

        var aciliyetDagilim = await db.AnalizSonuclari
            .GroupBy(s => s.AciliyetSeviyesi)
            .Select(g => new { Seviye = g.Key, Sayi = g.Count() })
            .ToListAsync();

        var kullanicilar = await userManager.Users
            .OrderByDescending(u => u.KayitTarihi)
            .Take(20)
            .ToListAsync();

        var sonAnalizler = await db.AnalizOturumlari
            .Include(o => o.AnalizSonucu)
            .Include(o => o.AnalizSemptomlari)
                .ThenInclude(s => s.SemptomKatalog)
            .Include(o => o.Kullanici)
            .OrderByDescending(o => o.OlusturulmaTarihi)
            .Take(10)
            .ToListAsync();

        ViewBag.ToplamKullanici = toplamKullanici;
        ViewBag.ToplamAnaliz = toplamAnaliz;
        ViewBag.BugunGiris = bugunGiris;
        ViewBag.AcilSayisi = aciliyetDagilim.FirstOrDefault(d => d.Seviye == AciliyetSeviyesi.Acil)?.Sayi ?? 0;
        ViewBag.Kullanicilar = kullanicilar;
        ViewBag.SonAnalizler = sonAnalizler;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> KullaniciSil(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["Hata"] = "Silinecek kullanıcı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        if (id == userManager.GetUserId(User))
        {
            TempData["Hata"] = "Kendi admin hesabınızı silemezsiniz.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var kullanici = await userManager.FindByIdAsync(id);
            if (kullanici == null)
            {
                TempData["Hata"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var sonuc = await userManager.DeleteAsync(kullanici);
            TempData[sonuc.Succeeded ? "Basarili" : "Hata"] = sonuc.Succeeded
                ? "Kullanıcı başarıyla silindi."
                : "Kullanıcı silinemedi.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin kullanıcı silme işleminde hata oluştu. KullaniciId: {KullaniciId}", id);
            TempData["Hata"] = "Kullanıcı silinirken beklenmeyen bir hata oluştu.";
            return RedirectToAction(nameof(Index));
        }
    }
}
