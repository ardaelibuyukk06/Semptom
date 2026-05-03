using SemptomAnalizApp.Core.Entities;

namespace SemptomAnalizApp.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<SaglikProfili> SaglikProfilleri { get; }
    IGenericRepository<SemptomKatalog> SemptomKatalog { get; }
    IGenericRepository<AnalizOturumu> AnalizOturumlari { get; }
    IGenericRepository<AnalizSemptomu> AnalizSemptomlari { get; }
    IGenericRepository<AnalizSonucu> AnalizSonuclari { get; }
    IGenericRepository<OlasiDurum> OlasiDurumlar { get; }

    // Teşhis motoru
    IGenericRepository<Hastalik> Hastaliklar { get; }
    IGenericRepository<Semptom>  Semptomlar  { get; }

    Task<int> SaveChangesAsync();
}
