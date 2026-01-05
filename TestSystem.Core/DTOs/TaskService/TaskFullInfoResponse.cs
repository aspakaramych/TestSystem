namespace TestSystem.Core.DTOs.TaskService;

public class TaskFullInfoResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string? InputSample { get; set; }
    public string? OutputSample { get; set; }
}