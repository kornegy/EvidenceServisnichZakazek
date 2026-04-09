namespace EvidenceServisnichZakazek.Models;
using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{   
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    public string? Email { get; set; }
    
    [Required(ErrorMessage = "FullName is required")]
    public string? FullName { get; set; }
    
    [Required(ErrorMessage = "PhoneNumber is required")]
    [RegularExpression(@"^\+?[0-9]{9,15}$", ErrorMessage = "Wrong number format! Example (+420123456789)")]
    public string? PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }
    
    [Required(ErrorMessage = "ConfirmPassword is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword { get; set; }
}