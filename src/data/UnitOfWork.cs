using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Interfaces;
using SemptomAnalizApp.Data.Repositories;

namespace SemptomAnalizApp.Data;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IGenericRepository<SaglikProfili>? _saglikProfilleri;
    private IGenericRepository<SemptomKatalog>? _semptomKatalog;
    private IGenericRepository<AnalizOturumu>?  _analizOturumlari;
    private IGenericRepository<AnalizSemptomu>? _analizSemptomlari;
    private IGenericRepository<AnalizSonucu>?   _analizSonuclari;
    private IGenericRepository<OlasiDurum>?     _olasiDurumlar;
    private IGenericRepository<Hastalik>?       _hastaliklar;
    private IGenericRepository<Semptom>?        _semptomlar;

    public IGenericRepository<SaglikProfili> SaglikProfilleri
        => _saglikProfilleri ??= new GenericRepository<SaglikProfili>(context);

    public IGenericRepository<SemptomKatalog> SemptomKatalog
        => _semptomKatalog ??= new GenericRepository<SemptomKatalog>(context);

    public IGenericRepository<AnalizOturumu> AnalizOturumlari
        => _analizOturumlari ??= new GenericRepository<AnalizOturumu>(context);

    public IGenericRepository<AnalizSemptomu> AnalizSemptomlari
        => _analizSemptomlari ??= new GenericRepository<AnalizSemptomu>(context);

    public IGenericRepository<AnalizSonucu> AnalizSonuclari
        => _analizSonuclari ??= new GenericRepository<AnalizSonucu>(context);

    public IGenericRepository<OlasiDurum> OlasiDurumlar
        => _olasiDurumlar ??= new GenericRepository<OlasiDurum>(context);

    public IGenericRepository<Hastalik> Hastaliklar
        => _hastaliklar ??= new GenericRepository<Hastalik>(context);

    public IGenericRepository<Semptom> Semptomlar
        => _semptomlar ??= new GenericRepository<Semptom>(context);

    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();

    public void Dispose() => context.Dispose();
}
