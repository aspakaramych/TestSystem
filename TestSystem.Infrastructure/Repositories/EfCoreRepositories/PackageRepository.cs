using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;

namespace TestSystem.Infrastructure.Repositories.EfCoreRepositories;

public class PackageRepository : Repository<Package>, IPackageRepository
{
    public PackageRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}