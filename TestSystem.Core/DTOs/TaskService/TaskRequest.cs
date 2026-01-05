using System.ComponentModel.DataAnnotations;

namespace TestSystem.Core.DTOs.TaskService;

public class TaskRequest
{
    [Required]
    public string Title { get; set; }
    [Required]
    public string Description { get; set; }
    public string? InputSample { get; set; }
    public string? OutputSample { get; set; }
    public string Tests { get; set; }
}