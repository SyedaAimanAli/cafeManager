using System.Linq.Expressions;
using RollUp.Core.Interfaces;
using RollUp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RollUp.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) =>
        await _set.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _set.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<T>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _set.AsNoTracking();
        foreach (var include in includes)
            query = query.Include(include);
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _set.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<IEnumerable<T>> FindWithIncludeAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _set.AsNoTracking().Where(predicate);
        foreach (var include in includes)
            query = query.Include(include);
        return await query.ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        await _set.FirstOrDefaultAsync(predicate);

    public async Task AddAsync(T entity) =>
        await _set.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<T> entities) =>
        await _set.AddRangeAsync(entities);

    public void Update(T entity) =>
        _set.Update(entity);

    public void Remove(T entity) =>
        _set.Remove(entity);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
        predicate is null
            ? await _set.CountAsync()
            : await _set.CountAsync(predicate);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
