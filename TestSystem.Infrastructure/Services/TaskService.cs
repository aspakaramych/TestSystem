using TestSystem.Core.DTOs.TaskService;
using TestSystem.Core.Entity;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Repositories.DapperRepositories;

namespace TestSystem.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly IDapperTaskEntityRepository _dapperTaskRepository;
    private readonly ITaskEntityRepository _taskRepository;
    
    public TaskService(IDapperTaskEntityRepository dapperTaskRepository, ITaskEntityRepository taskRepository)
    {
        _dapperTaskRepository = dapperTaskRepository;
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskResponse>> GetPaginatedAsync(Guid classroomId, int page = 1, int pageSize = 10)
    {
        var tasks = await _dapperTaskRepository.GetPaginatedByClassroomIdAsync(classroomId, page, pageSize);
        return tasks.Select(t => new TaskResponse
        {
            Id = t.Id,
            Title = t.Title,
        });
    }

    public async Task<TaskFullInfoResponse> GetFullInfoAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
        {
            throw new KeyNotFoundException($"Task with id {taskId} not found");
        }

        return new TaskFullInfoResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            InputSample = task.InputSample,
            OutputSample = task.OutputSample,
        };
    }

    public async Task CreateAsync(Guid classroomId, TaskRequest taskRequest)
    {
        
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = taskRequest.Title,
            Description = taskRequest.Description,
            InputSample = taskRequest.InputSample,
            OutputSample = taskRequest.OutputSample,
            Tests = taskRequest.Tests,
            ClassRoomId = classroomId,
        };
        await _taskRepository.AddAsync(task);
    }
}