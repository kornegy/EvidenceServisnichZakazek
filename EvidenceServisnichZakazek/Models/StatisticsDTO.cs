namespace EvidenceServisnichZakazek.Models;

public class StatisticsDTO
{
    public class StatusStatDto
    {
        public int Status { get; set; }
        public int TimesVisited { get; set; } 
        public double AvgMinutes { get; set; }
    }

    public class AppStatisticsDto
    {
        public int TotalOrders { get; set; }
        public List<StatusStatDto> StatusStats { get; set; } = new();
    }
}