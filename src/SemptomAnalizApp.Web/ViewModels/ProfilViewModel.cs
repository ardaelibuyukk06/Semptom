using System.ComponentModel.DataAnnotations;

namespace SemptomAnalizApp.Web.ViewModels;

public class ProfilViewModel
{
    [Required(ErrorMessage = "Yaş zorunludur.")]
    [Range(1, 120, ErrorMessage = "Geçerli bir yaş giriniz.")]
    [Display(Name = "Yaş")]
    public int Yas { get; set; }

    [Required(ErrorMessage = "Cinsiyet seçiniz.")]
    [Display(Name = "Cinsiyet")]
    public string Cinsiyet { get; set; } = string.Empty;

    [Required(ErrorMessage = "Boy zorunludur.")]
    [Range(50, 250, ErrorMessage = "Geçerli bir boy giriniz (cm).")]
    [Display(Name = "Boy (cm)")]
    public decimal Boy { get; set; }

    [Required(ErrorMessage = "Kilo zorunludur.")]
    [Range(10, 500, ErrorMessage = "Geçerli bir kilo giriniz (kg).")]
    [Display(Name = "Kilo (kg)")]
    public decimal Kilo { get; set; }

    [Display(Name = "Kronik Hastalıklar")]
    public List<string> SeciliKronikHastaliklar { get; set; } = [];

    [Display(Name = "Ek Notlar")]
    public string? Notlar { get; set; }

    // Hesaplanan (view'da kullanılır)
    public decimal? HesaplananBmi { get; set; }
    public string BmiKategoriMetni { get; set; } = string.Empty;
    public string BmiRengi { get; set; } = string.Empty;
}
