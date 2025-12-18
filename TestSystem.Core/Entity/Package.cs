using System.ComponentModel.DataAnnotations;

namespace TestSystem.Core.Entity;

public class Package
{
    [Key]
    public Guid Id { get; set; }
    public PackageStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public Language Language { get; set; }
    public string Code { get; set; }
    
    public TaskEntity Task { get; set; }
    public User User { get; set; }
}

public enum PackageStatus
{
    Pending,
    Accepted,
    Rejected,
}

public enum Language
{
    Cpp,
    CSharp,
    Python,
    Java,
}