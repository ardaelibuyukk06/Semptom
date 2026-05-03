using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;

namespace SemptomAnalizApp.Service.Interfaces;

public interface IAciliyetService
{
    int HesaplaSkor(
        List<SemptomGirdisi> girdiler,
        List<OlasiDurum> olasiDurumlar,
        SaglikProfili? profil,
        bool kritikVarMi);

    AciliyetSeviyesi SeviyeyeAta(int skor);
}
