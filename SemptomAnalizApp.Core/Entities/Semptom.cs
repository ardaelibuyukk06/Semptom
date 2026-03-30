namespace SemptomAnalizApp.Core.Entities;

/// <summary>
/// Sistemdeki semptomları temsil eder.
/// Kullanıcı analiz yaparken bu listeden seçim yapar.
/// </summary>
public class Semptom : BaseEntity
{
    // Semptomun adı — örn: "Ateş", "Öksürük", "Göğüs Ağrısı"
    public string Ad { get; set; } = string.Empty;

    // KritikMi = true ise bu semptom seçildiğinde normal hesaplama devre dışı kalır,
    // doğrudan acil uyarısı tetiklenir — örn: "Göğüs Ağrısı", "Nefes Darlığı"
    public bool KritikMi { get; set; } = false;

    // Semptomların kategori bazlı gruplanması için — örn: "Solunum", "Sindirim"
    public string Kategori { get; set; } = string.Empty;

    // Navigation property: bu semptoma bağlı hastalık-ağırlık kayıtları
    public ICollection<HastalikSemptom> HastalikSemptomlari { get; set; } = [];
}
