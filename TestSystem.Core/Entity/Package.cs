using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestSystem.Core.Entity;

public class Package
{
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