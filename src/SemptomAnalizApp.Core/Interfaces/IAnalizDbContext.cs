using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;

namespace SemptomAnalizApp.Core.Interfaces;

public interface IAnalizDbContext
{
    DbSet<SaglikProfili> SaglikProfilleri { get; }
    DbSet<SemptomKatalog> SemptomKatalog { get; }
    DbSet<AnalizOturumu> AnalizOturumlari { get; }
    DbSet<AnalizSemptomu> AnalizSemptomlari { get; }
    DbSet<AnalizSonucu> AnalizSonuclari { get; }
    DbSet<OlasiDurum> OlasiDurumlar { get; }
    DbSet<Hastalik> Hastaliklar { get; }
    DbSet<Semptom> Semptomlar { get; }
    DbSet<HastalikSemptom> HastalikSemptomlar { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
