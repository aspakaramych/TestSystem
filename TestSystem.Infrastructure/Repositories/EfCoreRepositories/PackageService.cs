using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;

namespace TestSystem.Infrastructure.Repositories.EfCoreRepositories;

public class PackageService : Repository<Package>, IPackageRepository
{
    public PackageService(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}