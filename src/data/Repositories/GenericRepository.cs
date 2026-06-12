using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Interfaces;

namespace SemptomAnalizApp.Data.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : class
{
    protected readonly DbSet<T> _set = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _set.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _set.Where(predicate).ToListAsync();

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}
