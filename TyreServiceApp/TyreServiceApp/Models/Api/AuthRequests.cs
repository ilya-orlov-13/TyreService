using System.ComponentModel.DataAnnotations;

namespace TyreServiceApp.Models.Api;

public class LoginRequest
{
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(4, MinimumLength = 4)]
    [RegularExpression(@"^\d{4}$")]
    public string Pin { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(4, MinimumLength = 4)]
    [RegularExpression(@"^\d{4}$")]
    public string Pin { get; set; } = string.Empty;
}
