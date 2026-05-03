using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;

namespace SemptomAnalizApp.Service.Interfaces;

public interface IAnalizMetinService
{
    string OlusturAciklama(
        List<SemptomGirdisi> girdiler,
        List<OlasiDurum> olasiDurumlar,
        SaglikProfili? profil,
        BmiKategori bmiKat);

    string OlusturGenelYorum(
        AciliyetSeviyesi seviye,
        List<OlasiDurum> olasiDurumlar,
        int tekrar,
        int? enYakinGun,
        SaglikProfili? profil);
}
