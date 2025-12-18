using Microsoft.EntityFrameworkCore;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;

namespace TestSystem.Infrastructure.Repositories.EfCoreRepositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
        return user;
    }
}