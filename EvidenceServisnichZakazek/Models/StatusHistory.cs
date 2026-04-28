namespace EvidenceServisnichZakazek.Models;

public class StatusHistory
{
    public int Id { get; set; }
    public int OrderId  { get; set; }
    public int StatusId { get; set; }
    public DateTime ChangedAt { get; set; }
}