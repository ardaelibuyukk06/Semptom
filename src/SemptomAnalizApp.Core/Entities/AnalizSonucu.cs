using SemptomAnalizApp.Core.Enums;

namespace SemptomAnalizApp.Core.Entities;

public class AnalizSonucu : BaseEntity
{
    public int AnalizOturumuId { get; set; }
    public AnalizOturumu AnalizOturumu { get; set; } = null!;

    public AciliyetSeviyesi AciliyetSeviyesi { get; set; }
    public int AciliyetSkoru { get; set; }       // 0–100
    public string OnerilenBolum { get; set; } = string.Empty;
    public string NedenAciklamasi { get; set; } = string.Empty;
    public string GenelYorum { get; set; } = string.Empty;  // Genel özet yorum

    // Tekrar tespiti
    public int TekrarSkoru { get; set; }  // kaç kez benzer semptom imzası görüldü
    public int? EnYakinTekrarGunOncesi { get; set; }

    // BMI
    public decimal HesaplananBmi { get; set; }
    public BmiKategori BmiKategori { get; set; }

    // JSON olarak saklanır
    public string GunlukOnerilerJson { get; set; } = "[]";
    public string UyariGostergeleriJson { get; set; } = "[]";

    // Claude AI bağımsız analizi — JSON olarak saklanır (opsiyonel)
    public string? ClaudeAnalizJson { get; set; }

    public ICollection<OlasiDurum> OlasiDurumlar { get; set; } = [];
}
