using SemptomAnalizApp.Core.Entities;

namespace SemptomAnalizApp.Service.Interfaces;

public record SemptomGirdisi(int SemptomId, int Siddet, int SureGun);

public interface IAnalizService
{
    Task<AnalizSonucu> AnalizEtAsync(
        string kullaniciId,
        IEnumerable<SemptomGirdisi> semptomlar,
        string? ekNotlar);
}
