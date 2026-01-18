using System.ComponentModel.DataAnnotations;

namespace TestSystem.Core.DTOs.PackageService;

public class PackageRequest
{
    [Required]
    public string Language { get; set; }
    [Required]
    public string Code { get; set; }
}