namespace SemptomAnalizApp.Core.Entities;

public class Hastalik : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public string OnerilenBolum { get; set; } = string.Empty;

    // Bayesian prior: nüfustaki görece yaygınlık (1–100)
    // Yüksek = yaygın (Grip: 80), Düşük = nadir (Menenjit: 5)
    public int Prevalans { get; set; } = 50;

    // Tipik yaş aralığı — null = tüm yaşlar
    public int? MinYas { get; set; }
    public int? MaxYas { get; set; }

    // Cinsiyete göre risk çarpanı (1.0 = nötr, 1.5 = 1.5x daha riskli)
    public decimal ErkekKatsayi { get; set; } = 1.0m;
    public decimal KadinKatsayi { get; set; } = 1.0m;

    // true = akut (1–14 gün tipik), false = kronik/subakut (14+ gün tipik)
    public bool AkutMu { get; set; } = false;

    public ICollection<HastalikSemptom> HastalikSemptomlari { get; set; } = [];
}
