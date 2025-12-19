using Dapper;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;

namespace TestSystem.Infrastructure.Repositories.DapperRepositories;

public class DapperClassRoomsRepository : IDapperClassRoomsRepository
{
    private readonly DapperDbContext _context;

    public DapperClassRoomsRepository(DapperDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClassRoom>?> GetClassRoomsAsync(Guid userId)
    {
        var sql = @"select cr.*
                    from ClassRooms cr
                    join UserClassRooms ucr on cr.Id = ucr.ClassRoomId
                    where ucr.UserId = @UserId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ClassRoom>(sql, new { UserId = userId });
    }
}