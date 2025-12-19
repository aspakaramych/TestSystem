using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;

namespace TestSystem.Infrastructure.Repositories.EfCoreRepositories;

public class ClassRoomsRepository : Repository<ClassRoom>, IClassRoomsRepository
{
    public ClassRoomsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}