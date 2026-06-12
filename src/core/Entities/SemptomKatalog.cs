using SemptomAnalizApp.Core.Enums;

namespace SemptomAnalizApp.Core.Entities;

public class SemptomKatalog : BaseEntity
{
    public string Ad { get; set; } = string.Empty;
    public SemptomKategorisi Kategori { get; set; }
    public string IkonKodu { get; set; } = "bi-heart-pulse"; // Bootstrap Icons
    public string? Aciklama { get; set; }
    public bool Aktif { get; set; } = true;

    // Teşhis motorundaki karşılık gelen Semptom kaydına FK.
    // Nullable: katalog UI'a özgü bir semptom eklenirse motor kaydı olmayabilir.
    public int? SemptomId { get; set; }
    public Semptom? Semptom { get; set; }

    public ICollection<AnalizSemptomu> AnalizSemptomlari { get; set; } = [];
}
