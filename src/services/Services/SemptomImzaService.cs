using System.Security.Cryptography;
using System.Text;
using SemptomAnalizApp.Service.Interfaces;

namespace SemptomAnalizApp.Service.Services;

public sealed class SemptomImzaService : ISemptomImzaService
{
    public string Olustur(List<int> semptomIdler) =>
        OlusturImza(semptomIdler);

    internal static string OlusturImza(List<int> semptomIdler)
    {
        var sirali = string.Join("|", semptomIdler.OrderBy(x => x));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sirali));
        return Convert.ToHexString(hash)[..12].ToLower();
    }
}
