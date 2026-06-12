using System.Text;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Service.Interfaces;

namespace SemptomAnalizApp.Service.Services;

public sealed class AnalizMetinService : IAnalizMetinService
{
    public string OlusturAciklama(
        List<SemptomGirdisi> girdiler,
        List<OlasiDurum> olasiDurumlar,
        SaglikProfili? profil,
        BmiKategori bmiKat)
    {
        var sb = new StringBuilder();

        if (olasiDurumlar.Count > 0)
            sb.Append($"{olasiDurumlar[0].Aciklama} ");

        var ortSure = girdiler.Any() ? girdiler.Average(g => g.SureGun) : 0;
        var ortSiddet = girdiler.Any() ? girdiler.Average(g => g.Siddet) : 0;

        sb.Append($"Girilen {girdiler.Count} semptomun ortalama {(int)ortSure} gündür sürmesi ve ");
        sb.Append(ortSiddet switch
        {
            < 1.5 => "hafif şiddette seyretmesi",
            < 2.5 => "orta şiddette seyretmesi",
            _ => "yüksek şiddette seyretmesi"
        });
        sb.Append(" benzerlik skorunu belirleyen başlıca faktörler arasındadır.");

        if (girdiler.Any(g => g.Siddet == 3))
        {
            var siddetliSayisi = girdiler.Count(g => g.Siddet == 3);
            sb.Append($" {siddetliSayisi} semptom şiddetli olarak işaretlendiğinden genel aciliyet skoru daha yüksek hesaplanmıştır.");
        }

        if (girdiler.Any(g => g.SureGun >= 14))
            sb.Append(" İki haftayı aşan semptomlar kronik hastalık ilişkisinin araştırılmasını önemli kılar.");

        if (bmiKat is BmiKategori.ObezeI or BmiKategori.ObezeII)
            sb.Append(" Yüksek VKİ değeri bazı durumların görülme sıklığını artırmaktadır.");

        if (!string.IsNullOrEmpty(profil?.KronikHastaliklar))
            sb.Append($" Kayıtlı kronik hastalıklar ({profil.KronikHastaliklar}) değerlendirmeye dahil edilmiştir.");

        if (profil?.Yas >= 65)
            sb.Append(" İleri yaş grubu için aciliyet eşiği daha düşük tutulmuştur.");

        sb.Append(" Analiz yalnızca sisteme girilmiş bilgilere dayanmaktadır; muayene, laboratuvar ve görüntüleme bulguları dahil değildir.");

        return sb.ToString();
    }

    public string OlusturGenelYorum(
        AciliyetSeviyesi seviye,
        List<OlasiDurum> olasiDurumlar,
        int tekrar,
        int? enYakinGun,
        SaglikProfili? profil)
    {
        var sb = new StringBuilder();

        sb.Append(seviye switch
        {
            AciliyetSeviyesi.Normal => "Genel tablonuz normal sınırlar içinde görünmektedir. ",
            AciliyetSeviyesi.Izle => "Belirtileriniz hafif-orta düzeyde seyrediyor; durumu takip etmeniz önerilir. ",
            AciliyetSeviyesi.Dikkat => "Semptomlarınız dikkat gerektiren bir tablo oluşturmaktadır; yakın zamanda bir sağlık profesyoneliyle görüşmeniz tavsiye edilir. ",
            AciliyetSeviyesi.Acil => "Mevcut bulgular acil tıbbi değerlendirme gerektirebilir; lütfen en kısa sürede bir sağlık kuruluşuna başvurun. ",
            _ => string.Empty
        });

        if (olasiDurumlar.Count > 0)
            sb.Append($"İstatistiksel benzerlik analizi en yüksek uyumu '{olasiDurumlar[0].Ad}' tablosuyla göstermektedir. ");

        if (tekrar > 0 && enYakinGun.HasValue)
            sb.Append($"Bu semptom kombinasyonu son 30 gün içinde {tekrar} kez kaydedilmiştir; {enYakinGun} gün önce benzer bir tablo mevcut. Tekrarlayan belirtiler altta yatan bir durumu düşündürebilir. ");

        if (profil != null && !string.IsNullOrEmpty(profil.KronikHastaliklar))
            sb.Append("Kronik hastalık geçmişiniz nedeniyle bulgularınızın bir uzman tarafından değerlendirilmesi özellikle önem taşımaktadır.");

        sb.Append(" Bu değerlendirme klinik teşhis veya tıbbi tavsiye değildir; yalnızca istatistiksel benzerlik skoruna dayanan bir karar destek çıktısıdır. Kesin değerlendirme için bir sağlık profesyoneline başvurunuz.");

        return sb.ToString();
    }
}
