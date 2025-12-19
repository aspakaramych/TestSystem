using TestSystem.Core.Entity;

namespace TestSystem.Core.Interfaces;

public interface IDapperClassRoomsRepository
{
    Task<IEnumerable<ClassRoom>?> GetClassRoomsAsync(Guid userId);
}