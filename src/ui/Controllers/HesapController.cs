using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Web.ViewModels;

namespace SemptomAnalizApp.Web.Controllers;

public class HesapController(
    UserManager<Kullanici> userManager,
    SignInManager<Kullanici> signInManager,
    ILogger<HesapController> logger) : Controller
{
    [HttpGet]
    public IActionResult Kayit() => View();

    [HttpPost]
    public async Task<IActionResult> Kayit(KayitViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var kullanici = new Kullanici
            {
                UserName = model.Email,
                Email = model.Email,
                Ad = model.Ad,
                Soyad = model.Soyad,
                EmailConfirmed = false
            };

            var sonuc = await userManager.CreateAsync(kullanici, model.Sifre);
            if (sonuc.Succeeded)
            {
                await userManager.AddToRoleAsync(kullanici, "Kullanici");

                var token = await userManager.GenerateEmailConfirmationTokenAsync(kullanici);
                var callbackUrl = Url.Action(
                    nameof(EmailDogrula),
                    "Hesap",
                    new { userId = kullanici.Id, token },
                    Request.Scheme);

                logger.LogInformation("E-posta doğrulama bağlantısı oluşturuldu: {ConfirmationLink}", callbackUrl);
                TempData["Basarili"] = "Hesabınız oluşturuldu. Giriş yapmadan önce e-posta adresinizi doğrulayın.";
                TempData["EmailDogrulamaLinki"] = callbackUrl;
                return RedirectToAction("Giris");
            }

            foreach (var hata in sonuc.Errors)
                ModelState.AddModelError(string.Empty, hata.Description);

            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Kullanıcı kaydı sırasında hata oluştu.");
            ModelState.AddModelError(string.Empty, "Kayıt sırasında beklenmeyen bir hata oluştu. Lütfen tekrar deneyiniz.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EmailDogrula(string? userId, string? token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            TempData["Hata"] = "E-posta doğrulama bağlantısı geçersiz.";
            return RedirectToAction("Giris");
        }

        var kullanici = await userManager.FindByIdAsync(userId);
        if (kullanici == null)
        {
            TempData["Hata"] = "E-posta doğrulama kullanıcısı bulunamadı.";
            return RedirectToAction("Giris");
        }

        var sonuc = await userManager.ConfirmEmailAsync(kullanici, token);
        TempData[sonuc.Succeeded ? "Basarili" : "Hata"] = sonuc.Succeeded
            ? "E-posta adresiniz doğrulandı. Artık giriş yapabilirsiniz."
            : "E-posta doğrulama işlemi tamamlanamadı.";

        return RedirectToAction("Giris");
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

        try
        {
            var sonuc = await signInManager.PasswordSignInAsync(
                model.Email, model.Sifre, model.BeniHatirla, lockoutOnFailure: true);

            if (sonuc.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Dashboard", "Home");
            }

            if (sonuc.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Çok fazla hatalı deneme yapıldı. Lütfen 10 dakika sonra tekrar deneyiniz.");
                return View(model);
            }

            if (sonuc.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Giriş yapmadan önce e-posta adresinizi doğrulamanız gerekir.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Giriş sırasında hata oluştu.");
            ModelState.AddModelError(string.Empty, "Giriş sırasında beklenmeyen bir hata oluştu. Lütfen tekrar deneyiniz.");
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Cikis()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
