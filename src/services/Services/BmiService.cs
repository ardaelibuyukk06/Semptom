using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;
using SemptomAnalizApp.Service.Interfaces;

namespace SemptomAnalizApp.Service.Services;

public sealed class BmiService : IBmiService
{
    public (decimal bmi, BmiKategori kat) Hesapla(SaglikProfili? profil) =>
        HesaplaBmi(profil);

    internal static (decimal bmi, BmiKategori kat) HesaplaBmi(SaglikProfili? profil)
    {
        if (profil == null || profil.Boy <= 0 || profil.Kilo <= 0)
            return (0, BmiKategori.Normal);

        var boyM = profil.Boy / 100m;
        var bmi = Math.Round(profil.Kilo / (boyM * boyM), 1);
        var kat = bmi switch
        {
            < 18.5m => BmiKategori.ZayifAltinda,
            < 25.0m => BmiKategori.Normal,
            < 30.0m => BmiKategori.Fazlakilolu,
            < 35.0m => BmiKategori.ObezeI,
            _ => BmiKategori.ObezeII
        };

        return (bmi, kat);
    }
}
