namespace TestSystem.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task<T?> GetByIdAsync(Guid id);
    Task<ICollection<T>> GetAllAsync();
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}