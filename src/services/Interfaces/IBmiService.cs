using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Enums;

namespace SemptomAnalizApp.Service.Interfaces;

public interface IBmiService
{
    (decimal bmi, BmiKategori kat) Hesapla(SaglikProfili? profil);
}
