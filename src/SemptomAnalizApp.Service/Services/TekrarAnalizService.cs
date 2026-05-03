using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Interfaces;
using SemptomAnalizApp.Service.Interfaces;

namespace SemptomAnalizApp.Service.Services;

public sealed class TekrarAnalizService(IAnalizDbContext db) : ITekrarAnalizService
{
    public async Task<(int tekrar, int? enYakinGun)> TespitEtAsync(string kullaniciId, string imza)
    {
        var sinir = DateTime.UtcNow.AddDays(-30);
        var eskiler = await db.AnalizOturumlari
            .Where(o => o.KullaniciId == kullaniciId
                     && o.SemptomImzasi == imza
                     && o.OlusturulmaTarihi >= sinir)
            .OrderByDescending(o => o.OlusturulmaTarihi)
            .ToListAsync();

        if (!eskiler.Any()) return (0, null);

        var enYakin = (int)(DateTime.UtcNow - eskiler.First().OlusturulmaTarihi).TotalDays;
        return (eskiler.Count, enYakin);
    }
}
