namespace TestSystem.Core.DTOs.PackageService;

public class PackageResponse
{
    public Guid Id { get; set; }
    public string TaskTitle { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Language { get; set; }
    public string Status { get; set; }
}