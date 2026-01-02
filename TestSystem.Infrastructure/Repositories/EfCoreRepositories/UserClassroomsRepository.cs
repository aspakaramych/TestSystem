using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;

namespace TestSystem.Infrastructure.Repositories.EfCoreRepositories;

public class UserClassroomsRepository : Repository<UserClassRoom>, IUserClassroomsRepository
{
    public UserClassroomsRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}