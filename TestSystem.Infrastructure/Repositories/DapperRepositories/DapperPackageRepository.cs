using Dapper;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;

namespace TestSystem.Infrastructure.Repositories.DapperRepositories;

public class DapperPackageRepository : IDapperPackageRepository
{
    private readonly DapperDbContext _dbContext;

    public DapperPackageRepository(DapperDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<ICollection<Package>> GetPackagesAsync(int page, int pageSize, Guid userId)
    {
        var offset = (page - 1) * pageSize;
        var sql = @"
        select 
            p.id, p.status, p.created_at, p.language, p.code, p.task_id, p.user_id,
            t.id, t.title 
        from packages p
        join tasks t on p.task_id = t.id
        where p.user_id = @userId
        order by p.created_at desc
        OFFSET @offset ROWS 
        FETCH NEXT @pageSize ROWS ONLY";
        using var connection = _dbContext.CreateConnection();
        var packages = await connection.QueryAsync<Package, TaskEntity, Package>(
            sql,
            (package, task) =>
            {
                package.Task = task;
                return package;
            },
            new { userId, offset, pageSize },
            splitOn: "id"
        );
        return packages.ToList();
    }
}