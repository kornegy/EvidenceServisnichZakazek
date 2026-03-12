using System.ComponentModel.DataAnnotations;

namespace EvidenceServisnichZakazek.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your email")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string? Email { get; set; }
    
    [Required(ErrorMessage = "Enter your password")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}