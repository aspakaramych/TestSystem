using System.ComponentModel.DataAnnotations;

namespace TestSystem.Core.DTOs.ClassRoomService;

public class ClassRoomCreateRequest
{
    [Required]
    public string Title { get; set; }
}