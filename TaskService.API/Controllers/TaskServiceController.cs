using Microsoft.AspNetCore.Mvc;
using TestSystem.Core.DTOs.TaskService;
using TestSystem.Core.Interfaces;

namespace TaskService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskServiceController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TaskServiceController> _logger;

    public TaskServiceController(ITaskService taskService, ILogger<TaskServiceController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet("health")]
    public Task<IActionResult> Health()
    {
        _logger.LogInformation("Health check endpoint called.");
        return Task.FromResult<IActionResult>(Ok(new { status = "Healthy" }));
    }

    [HttpGet("{classroomId}/tasks/{id}")]
    public async Task<IActionResult> GetTask(Guid classroomId, Guid id)
    {
        var task = await _taskService.GetFullInfoAsync(id);
        return Ok(task);
    }

    [HttpPost("{classroomId}/tasks")]
    public async Task<IActionResult> CreateTask(Guid classroomId, [FromBody] TaskRequest request)
    {
        await _taskService.CreateAsync(classroomId, request);
        return StatusCode(201);
    }

    [HttpGet("{classroomId}/tasks")]
    public async Task<IActionResult> GetTasks(Guid classroomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var tasks = await _taskService.GetPaginatedAsync(classroomId, page, pageSize);
        return Ok(tasks);
    }
}