using Dapper;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;

namespace TestSystem.Infrastructure.Repositories.DapperRepositories;

public class DapperTaskEntityRepository : IDapperTaskEntityRepository
{
    private readonly DapperDbContext _context;

    public DapperTaskEntityRepository(DapperDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskEntity>> GetByClassroomIdAsync(Guid classroomId)
    {
        var sql = @"select t.id, t.title from tasks as t where classroomId = @classroomId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<TaskEntity>(sql, new { classroomId = classroomId });
    }

    public async Task<IEnumerable<TaskEntity>> GetPaginatedByClassroomIdAsync(Guid classroomId, int page, int pageSize)
    {
        var sql =
            @"select t.id, t.title from tasks as t where classroomId = @classroomId order by t.id offset (@page - 1) * @pageSize rows fetch next @pageSize rows only";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<TaskEntity>(sql, new { classroomId, page, pageSize });
    }
}