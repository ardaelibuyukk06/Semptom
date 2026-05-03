namespace SemptomAnalizApp.Core.Entities;

public class AnalizOturumu : BaseEntity
{
    public string KullaniciId { get; set; } = string.Empty;
    public Kullanici Kullanici { get; set; } = null!;

    public string? EkNotlar { get; set; }

    // Semptom İmzası: seçilen semptom ID'lerinin sıralı hash'i
    // Örn: "3|7|12|18" → SHA256 kısaltması
    public string SemptomImzasi { get; set; } = string.Empty;

    public ICollection<AnalizSemptomu> AnalizSemptomlari { get; set; } = [];
    public AnalizSonucu? AnalizSonucu { get; set; }
}
