namespace SemptomAnalizApp.Core.Entities;

/// <summary>
/// Hastalik ile Semptom arasındaki many-to-many ilişkiyi temsil eder.
/// Basit bir join tablosu değil — içindeki Agirlik değeri
/// teşhis algoritmasının temelini oluşturur.
///
/// Örnek: Grip + Ateş → Agirlik: 9 (çok güçlü ilişki)
///        Grip + Baş Ağrısı → Agirlik: 6 (orta ilişki)
/// </summary>
public class HastalikSemptom
{
    // Bağlı olduğu hastalığın ID'si (foreign key)
    public int HastalikId { get; set; }

    // Bağlı olduğu semptomun ID'si (foreign key)
    public int SemptomId { get; set; }

    // Ağırlık puanı: 1 (zayıf ilişki) — 10 (çok güçlü ilişki)
    // Teşhis algoritması bu puanları toplayarak hastalık olasılığını hesaplar
    public int Agirlik { get; set; }

    // Navigation properties — EF Core ilişkileri bu sayede kurar
    public Hastalik Hastalik { get; set; } = null!;
    public Semptom  Semptom  { get; set; } = null!;
}
