using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Core.Interfaces;
using SemptomAnalizApp.Service.Interfaces;

namespace SemptomAnalizApp.Service.Services;

public sealed class BayesianAnalizService(IAnalizDbContext db) : IBayesianAnalizService
{
    private const double P_S_GIVEN_NOT_D = 0.07;

    public async Task<List<OlasiDurum>> HesaplaOlasiDurumlarAsync(
        HashSet<int> motorSemptomSet,
        List<SemptomGirdisi> motorGirdileri,
        int yas,
        string cinsiyet,
        double ortSureGun,
        BmiKategori bmiKat)
    {
        if (!motorSemptomSet.Any()) return [];

        var eslesen = await db.HastalikSemptomlar
            .Where(hs => motorSemptomSet.Contains(hs.SemptomId))
            .Select(hs => hs.HastalikId)
            .Distinct()
            .ToListAsync();

        if (!eslesen.Any()) return [];

        var hastalikSemptomlar = await db.HastalikSemptomlar
            .Include(hs => hs.Hastalik)
            .Where(hs => eslesen.Contains(hs.HastalikId))
            .ToListAsync();

        var sonuclar = new List<(OlasiDurum durum, double logPosterior)>();

        foreach (var grup in hastalikSemptomlar.GroupBy(hs => hs.HastalikId))
        {
            var hastalik = grup.First().Hastalik;
            var logPrior = Math.Log(hastalik.Prevalans + 1.0);
            var logLikelihoodRatio = 0.0;
            var seciliSayi = 0;

            foreach (var hs in grup)
            {
                var pSGivenD = hs.Agirlik / 10.0 * 0.72 + 0.13;

                if (motorSemptomSet.Contains(hs.SemptomId))
                {
                    logLikelihoodRatio += Math.Log(pSGivenD / P_S_GIVEN_NOT_D);
                    seciliSayi++;
                }
                else if (hs.Agirlik >= 7)
                {
                    var pAbsGivenD = 1.0 - pSGivenD;
                    var pAbsGivenNotD = 1.0 - P_S_GIVEN_NOT_D;
                    logLikelihoodRatio += 0.4 * Math.Log(pAbsGivenD / pAbsGivenNotD);
                }
            }

            if (seciliSayi == 0) continue;

            var ageMod = HesaplaYasModu(hastalik, yas);
            var sexMod = HesaplaCinsiyetModu(hastalik, cinsiyet);
            var durMod = HesaplaSureModu(hastalik, ortSureGun);
            var siddetMod = HesaplaSiddetModu(grup, motorGirdileri);
            var bmiMod = bmiKat is BmiKategori.ObezeI or BmiKategori.ObezeII ? 1.12 : 1.0;

            var logPosterior = logPrior + logLikelihoodRatio
                + Math.Log(Math.Max(1e-10, ageMod * sexMod * durMod * siddetMod * bmiMod));

            sonuclar.Add((new OlasiDurum
            {
                Ad = hastalik.Ad,
                Aciklama = hastalik.Aciklama,
                HastalikId = hastalik.Id
            }, logPosterior));
        }

        if (!sonuclar.Any()) return [];

        var maxLog = sonuclar.Max(x => x.logPosterior);
        var expList = sonuclar.Select(x => Math.Exp(x.logPosterior - maxLog)).ToList();
        var sumExp = expList.Sum();

        for (var i = 0; i < sonuclar.Count; i++)
        {
            sonuclar[i].durum.SkorYuzdesi = Math.Max(1, (int)Math.Round(expList[i] / sumExp * 100));
        }

        return [.. sonuclar
            .OrderByDescending(x => x.logPosterior)
            .Take(5)
            .Select(x => x.durum)];
    }

    private static double HesaplaYasModu(Hastalik hastalik, int yas)
    {
        var ageMod = 1.0;
        if (yas <= 0) return ageMod;

        if (hastalik.MinYas.HasValue && yas < hastalik.MinYas.Value)
            ageMod *= Math.Max(0.3, 1.0 - (hastalik.MinYas.Value - yas) * 0.025);

        if (hastalik.MaxYas.HasValue && yas > hastalik.MaxYas.Value)
            ageMod *= Math.Max(0.4, 1.0 - (yas - hastalik.MaxYas.Value) * 0.02);

        return ageMod;
    }

    private static double HesaplaCinsiyetModu(Hastalik hastalik, string cinsiyet)
    {
        var erkekMod = hastalik.ErkekKatsayi > 0 ? (double)hastalik.ErkekKatsayi : 1.0;
        var kadinMod = hastalik.KadinKatsayi > 0 ? (double)hastalik.KadinKatsayi : 1.0;

        return cinsiyet == "Erkek" ? erkekMod
            : cinsiyet == "Kadın" ? kadinMod
            : 1.0;
    }

    private static double HesaplaSureModu(Hastalik hastalik, double ortSureGun)
    {
        if (hastalik.AkutMu)
        {
            return ortSureGun <= 7 ? 1.25
                : ortSureGun <= 14 ? 1.0
                : ortSureGun <= 30 ? 0.65
                : 0.35;
        }

        return ortSureGun >= 30 ? 1.3
            : ortSureGun >= 14 ? 1.1
            : ortSureGun >= 7 ? 0.9
            : 0.7;
    }

    private static double HesaplaSiddetModu(
        IEnumerable<HastalikSemptom> hastalikSemptomlari,
        List<SemptomGirdisi> motorGirdileri)
    {
        var semptomSet = hastalikSemptomlari.Select(hs => hs.SemptomId).ToHashSet();
        var ortSiddet = motorGirdileri
            .Where(g => semptomSet.Contains(g.SemptomId))
            .DefaultIfEmpty()
            .Average(g => g?.Siddet ?? 2.0);

        return 0.80 + ortSiddet * 0.10;
    }
}
