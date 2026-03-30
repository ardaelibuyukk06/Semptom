using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Web.ViewModels;

namespace SemptomAnalizApp.Web.Controllers;

public class HesapController(UserManager<Kullanici> userManager, SignInManager<Kullanici> signInManager) : Controller
{
    [HttpGet]
    public IActionResult Kayit() => View();

    [HttpPost]
    public async Task<IActionResult> Kayit(KayitViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var kullanici = new Kullanici
        {
            UserName = model.Email,
            Email = model.Email,
            Ad = model.Ad,
            Soyad = model.Soyad,
            EmailConfirmed = true
        };

        var sonuc = await userManager.CreateAsync(kullanici, model.Sifre);
        if (sonuc.Succeeded)
        {
            await userManager.AddToRoleAsync(kullanici, "Kullanici");
            await signInManager.SignInAsync(kullanici, isPersistent: false);
            TempData["KayitBasarili"] = model.Ad;
            return RedirectToAction("Dashboard", "Home");
        }

        foreach (var hata in sonuc.Errors)
            ModelState.AddModelError(string.Empty, hata.Description);

        return View(model);
    }

    [HttpGet]
    public IActionResult Giris(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Giris(GirisViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var sonuc = await signInManager.PasswordSignInAsync(
            model.Email, model.Sifre, model.BeniHatirla, lockoutOnFailure: false);

        if (sonuc.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Dashboard", "Home");
        }

        ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Cikis()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
