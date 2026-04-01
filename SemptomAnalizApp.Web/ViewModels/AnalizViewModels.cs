using SemptomAnalizApp.Core.Enums;

namespace SemptomAnalizApp.Web.ViewModels;

// ── Yeni Analiz Girişi ──────────────────────────────────────────────
public class YeniAnalizViewModel
{
    public List<SemptomGrubu> SemptomGruplari { get; set; } = [];
    public string? EkNotlar { get; set; }
}

public class SemptomGrubu
{
    public string KategoriAdi { get; set; } = string.Empty;
    public string IkonKodu { get; set; } = string.Empty;
    public List<SemptomSecenegi> Semptomlar { get; set; } = [];
}

public class SemptomSecenegi
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string IkonKodu { get; set; } = string.Empty;
}

// ── Analiz Sonucu ───────────────────────────────────────────────────
public class AnalizSonucViewModel
{
    public int OturumId { get; set; }
    public DateTime Tarih { get; set; }

    // Aciliyet
    public int AciliyetSkoru { get; set; }
    public string AciliyetEtiketi { get; set; } = string.Empty;
    public string AciliyetRengi { get; set; } = string.Empty;
    public string AciliyetArkaPlan { get; set; } = string.Empty;
    public string OnerilenBolum { get; set; } = string.Empty;

    // Açıklama & genel yorum
    public string NedenAciklamasi { get; set; } = string.Empty;
    public string GenelYorum { get; set; } = string.Empty;

    // Semptom imzası & tekrar
    public string SemptomImzasi { get; set; } = string.Empty;
    public int TekrarSkoru { get; set; }
    public int? EnYakinTekrarGunOncesi { get; set; }

    // BMI
    public decimal Bmi { get; set; }
    public string BmiKategoriMetni { get; set; } = string.Empty;
    public string BmiRengi { get; set; } = string.Empty;
    public int BmiYuzdesi { get; set; }

    // Olası durumlar
    public List<OlasiDurumSatiri> OlasiDurumlar { get; set; } = [];

    // Öneriler & uyarılar
    public List<GunlukOneriItem> GunlukOneriler { get; set; } = [];
    public List<string> UyariGostergeleri { get; set; } = [];

    // Seçilen semptomlar (özet için)
    public List<string> SecilmisSemptomlar { get; set; } = [];

    // Claude AI bağımsız analizi (null ise bölüm gösterilmez)
    public string? ClaudeYorumu { get; set; }
    public List<ClaudeAnalizSatiri> ClaudeAnalizDurumlar { get; set; } = [];

    // Semptom parmak izi radar grafiği için sabit sıralı kategori verisi
    public List<string> RadarEtiketler { get; set; } = [];
    public List<int>    RadarVeriler   { get; set; } = [];
}

public class OlasiDurumSatiri
{
    public string Ad { get; set; } = string.Empty;
    public int SkorYuzdesi { get; set; }       // Bar genişliği için (0–100, göreli)
    public string Aciklama { get; set; } = string.Empty;
    public string BarRengi { get; set; } = string.Empty;
    public string BenzerlikEtiketi { get; set; } = string.Empty;  // "Yüksek Uyum" vb.
    public string EtiketRengi { get; set; } = string.Empty;       // Bootstrap renk sınıfı
}

public class GunlukOneriItem
{
    public string Ikon { get; set; } = string.Empty;
    public string Baslik { get; set; } = string.Empty;
    public string Metin { get; set; } = string.Empty;
}

public class ClaudeAnalizSatiri
{
    public string Ad { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public string UyumDuzeyi { get; set; } = string.Empty;
    public string UyumRengi { get; set; } = string.Empty;   // Bootstrap renk sınıfı
}
