namespace EvidenceServisnichZakazek.Models;

public class ServiceOrders
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int? TechniciansId { get; set; } //muze byt NULL
    
    public string? PhoneType { get; set; }
    public string? IssueDescription { get; set; }
    public double? Price { get; set; } // muze byt NULL
    public int CurrStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}