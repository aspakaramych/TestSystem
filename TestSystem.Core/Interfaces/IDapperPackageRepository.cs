using TestSystem.Core.Entity;

namespace TestSystem.Core.Interfaces;

public interface IDapperPackageRepository
{
    Task<ICollection<Package>> GetPackagesAsync(int page, int pageSize, Guid userId);
}