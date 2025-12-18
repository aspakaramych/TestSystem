using System.ComponentModel.DataAnnotations;

namespace TestSystem.Core.Entity;

public class User
{
    [Key]
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string HashPassword { get; set; }
    public UserRole Role { get; set; }
    
    public IEnumerable<UserClassRoom> UserClassRooms { get; set; }
    public IEnumerable<Package> Packages { get; set; }
}

public enum UserRole
{
    User,
    Admin,
}