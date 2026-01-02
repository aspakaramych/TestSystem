using System.ComponentModel.DataAnnotations.Schema;

namespace TestSystem.Core.Entity;


public class UserClassRoom
{
    public Guid UserId { get; set; }
    public Guid ClassRoomId { get; set; }

    public UserRoleInClassRoom Role { get; set; }
    
    public User User { get; set; }
    public ClassRoom ClassRoom { get; set; }
}

public enum UserRoleInClassRoom
{
    Student,
    Teacher,
}