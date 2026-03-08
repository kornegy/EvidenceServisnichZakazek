namespace EvidenceServisnichZakazek.Models;

public class OrderHistories
{
    public int Id {get; set;}
    public int OrderId {get; set;}
    public int Status { get; set; }
    public DateTime ChangedAt { get; set; }
    public int DurationMinutes { get; set; }
}