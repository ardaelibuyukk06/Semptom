using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Service.Interfaces;

namespace SemptomAnalizApp.Service.Services;

public sealed class GunlukOneriService : IGunlukOneriService
{
    private static readonly int[] KasEklemSemptomIdleri = [17, 18, 19, 20];

    public List<GunlukOneriDto> BelirleGunlukOneriler(
        List<int> semptomIdler,
        AciliyetSeviyesi seviye,
        bool kritikVarMi,
        IReadOnlyCollection<int> acilOneriSemptomIdleri)
    {
        var oneriler = new List<GunlukOneriDto>
        {
            new("💧", "Sıvı Tüketimi", "Günde en az 2-3 litre su için."),
            new("🛌", "Yeterli Dinlenme", "Düzenli ve yeterli uyku iyileşmeyi destekler."),
        };

        if (semptomIdler.Contains(21))
            oneriler.Add(new("🌡️", "Ateş Takibi", "Sabah ve akşam ateşinizi ölçüp not alın."));

        if (semptomIdler.Contains(6))
            oneriler.Add(new("🫖", "Sıcak İçecek", "Ihlamur veya zencefilli içecekler gırtlağı rahatlatabilir."));

        if (semptomIdler.Intersect(KasEklemSemptomIdleri).Any())
            oneriler.Add(new("🧘", "Hafif Hareket", "Ağır aktiviteden kaçının; hafif germe hareketleri yapabilirsiniz."));

        if (semptomIdler.Intersect([11, 12, 13, 14]).Any())
            oneriler.Add(new("🥗", "Hafif Beslenme", "Yağlı ve ağır yiyeceklerden uzak durarak bağırsak sistemini dinlendirin."));

        if (kritikVarMi || semptomIdler.Intersect(acilOneriSemptomIdleri).Any())
            oneriler.Add(new("🏥", "Acil Değerlendirme", "Bu semptomlar için vakit kaybetmeden bir sağlık kuruluşuna başvurun."));
        else if (seviye is AciliyetSeviyesi.Normal or AciliyetSeviyesi.Izle)
            oneriler.Add(new("🚶", "Kısa Yürüyüş", "Belirtileriniz hafifse kısa süreli yürüyüş faydalı olabilir."));

        return oneriler;
    }

    public List<string> BelirleUyarilar(
        List<int> semptomIdler,
        AciliyetSeviyesi seviye,
        bool kritikVarMi)
    {
        var uyarilar = new List<string>
        {
            "Belirtileriniz 5 günden uzun sürer veya kötüleşirse bir doktora başvurun.",
            "Ateş 39°C üzerine çıkarsa vakit kaybetmeden sağlık kuruluşuna gidin.",
        };

        if (kritikVarMi)
            uyarilar.Insert(0, "Göğüs ağrısı veya nefes darlığı kötüleşirse ACİL servisi arayın.");

        if (semptomIdler.Contains(3))
            uyarilar.Add("Ani gelişen şiddetli baş dönmesi veya görme bozukluğu acil değerlendirme gerektirir.");

        if (semptomIdler.Contains(13))
            uyarilar.Add("Kusma 24 saatten uzun sürerse veya kanlı kusma olursa acilen başvurun.");

        if (semptomIdler.Contains(26))
            uyarilar.Add("İstem dışı kilo kaybı devam ediyorsa mutlaka doktor kontrolü yaptırın.");

        if (seviye == AciliyetSeviyesi.Acil)
            uyarilar.Insert(0, "Aciliyet düzeyi yüksek - lütfen en kısa sürede bir sağlık profesyoneline görünün.");

        return uyarilar;
    }
}
