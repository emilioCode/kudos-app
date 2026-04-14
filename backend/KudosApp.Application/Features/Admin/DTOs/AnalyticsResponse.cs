namespace KudosApp.Application.Features.Admin.DTOs;

public class AnalyticsResponse
{
    public int TotalUsers { get; set; }
    public int TotalKudos { get; set; }
    public int TotalPoints { get; set; }
    public List<CategoryStats> KudosByCategory { get; set; } = [];
}
