using System.ComponentModel.DataAnnotations;

namespace EvidenceServisnichZakazek.Models;

public class CreateServiceOrderViewModel
{
    [Required(ErrorMessage = "Please choose a category.")]
    public string DeviceCategory { get; set; }
    
    [Required(ErrorMessage = "Please choose a model (ex. IPhone).")]
    public string DeviceModel { get; set; }
    
    [Required(ErrorMessage = "Describe the problem you're having.")]
    public string IssueDescription { get; set; }
}