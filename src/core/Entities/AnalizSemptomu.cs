using SemptomAnalizApp.Core.Enums;

namespace SemptomAnalizApp.Core.Entities;

public class AnalizSemptomu : BaseEntity
{
    public int AnalizOturumuId { get; set; }
    public AnalizOturumu AnalizOturumu { get; set; } = null!;

    public int SemptomKatalogId { get; set; }
    public SemptomKatalog SemptomKatalog { get; set; } = null!;

    public SemptomSiddeti Siddet { get; set; }
    public int SureGun { get; set; }
}
