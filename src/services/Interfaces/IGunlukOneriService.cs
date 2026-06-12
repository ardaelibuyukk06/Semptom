using SemptomAnalizApp.Core.Enums;

namespace SemptomAnalizApp.Service.Interfaces;

public record GunlukOneriDto(string Ikon, string Baslik, string Metin);

public interface IGunlukOneriService
{
    List<GunlukOneriDto> BelirleGunlukOneriler(
        List<int> semptomIdler,
        AciliyetSeviyesi seviye,
        bool kritikVarMi,
        IReadOnlyCollection<int> acilOneriSemptomIdleri);

    List<string> BelirleUyarilar(
        List<int> semptomIdler,
        AciliyetSeviyesi seviye,
        bool kritikVarMi);
}
