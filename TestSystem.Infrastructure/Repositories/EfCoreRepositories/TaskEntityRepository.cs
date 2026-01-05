using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;

namespace TestSystem.Infrastructure.Repositories.EfCoreRepositories;

public class TaskEntityRepository : Repository<TaskEntity>, ITaskEntityRepository
{
    public TaskEntityRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}