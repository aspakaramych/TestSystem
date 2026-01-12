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
        var sql = @"select t.id, t.title from tasks as t where classroomId = @ClassroomId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<TaskEntity>(sql, new { ClassroomId = classroomId });
    }

    public async Task<IEnumerable<TaskEntity>> GetPaginatedByClassroomIdAsync(Guid classroomId, int page, int pageSize)
    {
        var offset = (page - 1) * pageSize;
        var sql = @"
    SELECT t.id, t.title 
    FROM tasks AS t 
    WHERE class_room_id = @classroomId 
    ORDER BY t.id 
    OFFSET @offset ROWS 
    FETCH NEXT @pageSize ROWS ONLY";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<TaskEntity>(sql, new { classroomId, offset, pageSize });
    }
}