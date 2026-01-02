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
                    from class_rooms cr
                    join user_class_rooms ucr on cr.Id = ucr.class_room_id
                    where ucr.user_id = @UserId";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ClassRoom>(sql, new { UserId = userId });
    }
}