using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Core.Interfaces;
using SemptomAnalizApp.Service.Interfaces;
using SemptomAnalizApp.Service.Options;

namespace SemptomAnalizApp.Service.Services;

// AnalizMotoru ana akışı yönetir; hesaplama parçaları küçük servislerde tutulur.
public class AnalizMotoru(
    IAnalizDbContext db,
    IOptions<KritikSemptomOptions> kritikOptions,
    IBmiService bmiService,
    ISemptomImzaService semptomImzaService,
    IBayesianAnalizService bayesianAnalizService,
    IAciliyetService aciliyetService,
    ITekrarAnalizService tekrarAnalizService,
    IAnalizMetinService analizMetinService,
    IGunlukOneriService gunlukOneriService) : IAnalizService
{
    public async Task<AnalizSonucu> AnalizEtAsync(
        string kullaniciId,
        IEnumerable<SemptomGirdisi> semptomGirdileri,
        string? ekNotlar)
    {
        var girdiler = semptomGirdileri.ToList();
        var semptomIdler = girdiler.Select(g => g.SemptomId).ToList();

        var profil = await db.SaglikProfilleri
            .FirstOrDefaultAsync(p => p.KullaniciId == kullaniciId);

        var (bmi, bmiKat) = bmiService.Hesapla(profil);
        var imza = semptomImzaService.Olustur(semptomIdler);

        var katalogToMotorMap = await db.SemptomKatalog
            .Where(sk => semptomIdler.Contains(sk.Id) && sk.SemptomId != null)
            .Select(sk => new { KatalogId = sk.Id, MotorId = sk.SemptomId!.Value })
            .ToDictionaryAsync(x => x.KatalogId, x => x.MotorId);

        var motorSemptomIdler = katalogToMotorMap.Values.ToList();
        var motorSemptomSet = motorSemptomIdler.ToHashSet();

        var motorGirdileri = girdiler
            .Where(g => katalogToMotorMap.ContainsKey(g.SemptomId))
            .Select(g => g with { SemptomId = katalogToMotorMap[g.SemptomId] })
            .ToList();

        var kritikVarMi = motorSemptomIdler.Any() &&
            await db.Semptomlar.AnyAsync(s => s.KritikMi && motorSemptomIdler.Contains(s.Id));
        if (!kritikVarMi)
            kritikVarMi = semptomIdler.Any(id => kritikOptions.Value.KritikKatalogIdleri.Contains(id));

        var kullaniciYas = profil?.Yas ?? 0;
        var cinsiyet = profil?.Cinsiyet ?? "";
        var ortSureGun = girdiler.Any() ? girdiler.Average(g => g.SureGun) : 1.0;

        var olasiDurumlar = await bayesianAnalizService.HesaplaOlasiDurumlarAsync(
            motorSemptomSet,
            motorGirdileri,
            kullaniciYas,
            cinsiyet,
            ortSureGun,
            bmiKat);

        var aciliyetSkoru = aciliyetService.HesaplaSkor(girdiler, olasiDurumlar, profil, kritikVarMi);
        var aciliyetSeviyesi = aciliyetService.SeviyeyeAta(aciliyetSkoru);

        var (tekrarSkoru, enYakinGun) = await tekrarAnalizService.TespitEtAsync(kullaniciId, imza);
        var onerilenBolum = await GetOnerilenBolumAsync(olasiDurumlar);

        var aciklama = analizMetinService.OlusturAciklama(girdiler, olasiDurumlar, profil, bmiKat);
        var genelYorum = analizMetinService.OlusturGenelYorum(
            aciliyetSeviyesi,
            olasiDurumlar,
            tekrarSkoru,
            enYakinGun,
            profil);

        var gunlukOneriler = gunlukOneriService.BelirleGunlukOneriler(
            semptomIdler,
            aciliyetSeviyesi,
            kritikVarMi,
            kritikOptions.Value.AcilOneriKatalogIdleri);
        var uyarilar = gunlukOneriService.BelirleUyarilar(
            semptomIdler,
            aciliyetSeviyesi,
            kritikVarMi);

        var oturum = new AnalizOturumu
        {
            KullaniciId = kullaniciId,
            EkNotlar = ekNotlar,
            SemptomImzasi = imza
        };
        db.AnalizOturumlari.Add(oturum);

        var sonuc = new AnalizSonucu
        {
            AnalizOturumu = oturum,
            AciliyetSeviyesi = aciliyetSeviyesi,
            AciliyetSkoru = aciliyetSkoru,
            OnerilenBolum = onerilenBolum,
            NedenAciklamasi = aciklama,
            GenelYorum = genelYorum,
            TekrarSkoru = tekrarSkoru,
            EnYakinTekrarGunOncesi = enYakinGun,
            HesaplananBmi = bmi,
            BmiKategori = bmiKat,
            GunlukOnerilerJson = JsonSerializer.Serialize(gunlukOneriler),
            UyariGostergeleriJson = JsonSerializer.Serialize(uyarilar),
            ClaudeAnalizJson = null
        };
        db.AnalizSonuclari.Add(sonuc);

        foreach (var g in girdiler)
        {
            db.AnalizSemptomlari.Add(new AnalizSemptomu
            {
                AnalizOturumu = oturum,
                SemptomKatalogId = g.SemptomId,
                Siddet = (SemptomSiddeti)g.Siddet,
                SureGun = g.SureGun
            });
        }

        foreach (var d in olasiDurumlar)
        {
            d.AnalizSonucu = sonuc;
            db.OlasiDurumlar.Add(d);
        }

        await db.SaveChangesAsync();

        return sonuc;
    }

    private async Task<string> GetOnerilenBolumAsync(List<OlasiDurum> olasiDurumlar)
    {
        if (!olasiDurumlar.Any()) return "Aile Hekimliği";

        var ilk = olasiDurumlar[0];
        if (!ilk.HastalikId.HasValue) return "Aile Hekimliği";

        var hastalik = await db.Hastaliklar.FindAsync(ilk.HastalikId.Value);
        return hastalik?.OnerilenBolum ?? "Aile Hekimliği";
    }

    internal static (decimal bmi, BmiKategori kat) HesaplaBmi(SaglikProfili? profil) =>
        BmiService.HesaplaBmi(profil);

    internal static string OlusturImza(List<int> semptomIdler) =>
        SemptomImzaService.OlusturImza(semptomIdler);

    internal static int HesaplaAciliyetSkoru(
        List<SemptomGirdisi> girdiler,
        List<OlasiDurum> olasiDurumlar,
        SaglikProfili? profil,
        bool kritikVarMi) =>
        AciliyetService.HesaplaAciliyetSkoru(girdiler, olasiDurumlar, profil, kritikVarMi);

    internal static AciliyetSeviyesi SkoraSeviyeAta(int skor) =>
        AciliyetService.SkoraSeviyeAta(skor);
}
