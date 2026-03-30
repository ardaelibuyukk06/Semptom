namespace SemptomAnalizApp.Core.Entities;

public class SaglikProfili : BaseEntity
{
    public string KullaniciId { get; set; } = string.Empty;
    public Kullanici Kullanici { get; set; } = null!;

    public int Yas { get; set; }
    public string Cinsiyet { get; set; } = string.Empty; // Erkek / Kadın
    public decimal Boy { get; set; }   // cm
    public decimal Kilo { get; set; }  // kg
    public string? KronikHastaliklar { get; set; } // JSON ya da virgüllü
    public string? Notlar { get; set; }
}
