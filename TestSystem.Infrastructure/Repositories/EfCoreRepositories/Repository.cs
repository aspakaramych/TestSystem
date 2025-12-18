using Microsoft.EntityFrameworkCore;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;

namespace TestSystem.Infrastructure.Repositories.EfCoreRepositories;

public abstract class Repository<T> : IRepository<T> where T : class
{
    protected ApplicationDbContext _dbContext;

    public Repository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddAsync(T entity)
    {
        await _dbContext.Set<T>().AddAsync(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var result = await _dbContext.Set<T>().FindAsync(id);
        return result;
    }

    public async Task<ICollection<T>> GetAllAsync()
    {
        var result = await _dbContext.Set<T>().ToListAsync();
        return result;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbContext.Set<T>().Update(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}