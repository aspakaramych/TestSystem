using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestSystem.Core.Entity;

public class ClassRoom
{
    [Key]
    public Guid Id { get; set; }
    public string Title { get; set; }
    
    public IEnumerable<UserClassRoom> UserClassRooms { get; set; } 
    public IEnumerable<TaskEntity> Tasks { get; set; }
}