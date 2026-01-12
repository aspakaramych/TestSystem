using TestSystem.Core.Entity;

namespace TestSystem.Core.Interfaces;

public interface IDapperTaskEntityRepository
{
    Task<IEnumerable<TaskEntity>> GetByClassroomIdAsync(Guid classroomId);
    Task<IEnumerable<TaskEntity>> GetPaginatedByClassroomIdAsync(Guid classroomId, int page, int pageSize);
}