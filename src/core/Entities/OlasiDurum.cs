namespace SemptomAnalizApp.Core.Entities;

public class OlasiDurum : BaseEntity
{
    public int AnalizSonucuId { get; set; }
    public AnalizSonucu AnalizSonucu { get; set; } = null!;

    public string Ad { get; set; } = string.Empty;
    public int SkorYuzdesi { get; set; }  // 0–100
    public string Aciklama { get; set; } = string.Empty;

    // Sonucu üreten motor hastalık kaydına FK.
    // Nullable: hardcoded fallback yoluyla oluşturulan sonuçlar için null olabilir.
    public int? HastalikId { get; set; }
    public Hastalik? Hastalik { get; set; }
}
