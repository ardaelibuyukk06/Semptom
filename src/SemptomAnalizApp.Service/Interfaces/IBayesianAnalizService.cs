using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;

namespace SemptomAnalizApp.Service.Interfaces;

public interface IBayesianAnalizService
{
    Task<List<OlasiDurum>> HesaplaOlasiDurumlarAsync(
        HashSet<int> motorSemptomSet,
        List<SemptomGirdisi> motorGirdileri,
        int yas,
        string cinsiyet,
        double ortSureGun,
        BmiKategori bmiKat);
}
