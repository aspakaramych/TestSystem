using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestSystem.Core.Entity;


public class TaskEntity
{
    [Key]
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }
 
    public string? InputSample { get; set; }

    public string? OutputSample { get; set; }

    [DataType("JSONB")]
    public string Tests { get; set; }

    public Guid ClassRoomId { get; set; }
    public ClassRoom ClassRoom { get; set; }
    public IEnumerable<Package> Packages { get; set; }
}