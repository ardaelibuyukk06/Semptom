using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Service.Interfaces;

namespace SemptomAnalizApp.Service.Services;

public sealed class AciliyetService : IAciliyetService
{
    public int HesaplaSkor(
        List<SemptomGirdisi> girdiler,
        List<OlasiDurum> olasiDurumlar,
        SaglikProfili? profil,
        bool kritikVarMi) =>
        HesaplaAciliyetSkoru(girdiler, olasiDurumlar, profil, kritikVarMi);

    public AciliyetSeviyesi SeviyeyeAta(int skor) =>
        SkoraSeviyeAta(skor);

    internal static int HesaplaAciliyetSkoru(
        List<SemptomGirdisi> girdiler,
        List<OlasiDurum> olasiDurumlar,
        SaglikProfili? profil,
        bool kritikVarMi)
    {
        if (!girdiler.Any()) return 10;

        var bazSkor = girdiler.Average(g => g.Siddet * Math.Log(g.SureGun + 1) * 12.0);
        bazSkor = Math.Min(bazSkor, 65);

        if (olasiDurumlar.Count > 0)
            bazSkor = Math.Max(bazSkor, olasiDurumlar[0].SkorYuzdesi * 0.7);

        if (kritikVarMi)
            bazSkor = Math.Min(100, bazSkor + 22);

        if (!string.IsNullOrEmpty(profil?.KronikHastaliklar))
            bazSkor = Math.Min(100, bazSkor + 8);

        if (profil?.Yas >= 65)
            bazSkor = Math.Min(100, bazSkor + 5);

        return (int)Math.Ceiling(bazSkor);
    }

    internal static AciliyetSeviyesi SkoraSeviyeAta(int skor) => skor switch
    {
        <= 25 => AciliyetSeviyesi.Normal,
        <= 50 => AciliyetSeviyesi.Izle,
        <= 75 => AciliyetSeviyesi.Dikkat,
        _ => AciliyetSeviyesi.Acil
    };
}
