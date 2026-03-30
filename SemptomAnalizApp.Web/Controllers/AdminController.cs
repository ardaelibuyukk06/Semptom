using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Data;

namespace SemptomAnalizApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db, UserManager<Kullanici> userManager) : Controller
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
}
