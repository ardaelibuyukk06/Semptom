using System.ComponentModel.DataAnnotations;

namespace SemptomAnalizApp.Web.ViewModels;

public class KayitViewModel
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    [Display(Name = "Ad")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [Display(Name = "Soyad")]
    public string Soyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MinLength(8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir sembol içermelidir.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    public string SifreTekrar { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true",
        ErrorMessage = "Gizlilik politikasını kabul etmeniz zorunludur.")]
    [Display(Name = "Gizlilik Politikası")]
    public bool KvkkOnay { get; set; }

    [Range(typeof(bool), "true", "true",
        ErrorMessage = "Tıbbi sorumluluk reddi beyanını kabul etmeniz zorunludur.")]
    [Display(Name = "Tıbbi Sorumluluk Reddi")]
    public bool TibbiOnay { get; set; }
}

public class GirisViewModel
{
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = string.Empty;

    [Display(Name = "Beni hatırla")]
    public bool BeniHatirla { get; set; }
}
