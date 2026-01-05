using TestSystem.Core.DTOs.TaskService;
using TestSystem.Core.Entity;

namespace TestSystem.Core.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskResponse>> GetPaginatedAsync(Guid classroomId, int page, int pageSize);
    Task<TaskFullInfoResponse> GetFullInfoAsync(Guid taskId);
    Task CreateAsync(Guid classroomId, TaskRequest taskRequest);
}