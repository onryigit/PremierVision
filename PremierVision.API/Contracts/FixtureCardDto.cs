namespace PremierVision.Contracts;

public class FixtureCardDto
{
    public int Id { get; set; }
    public int MatchWeek { get; set; }
    public string HomeTeamName { get; set; } = string.Empty;
    public string? HomeTeamLogoUrl { get; set; }
    public string AwayTeamName { get; set; } = string.Empty;
    public string? AwayTeamLogoUrl { get; set; }
    public DateTime KickoffUtc { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public FixtureStatus Status { get; set; }
    public int? CurrentMinute { get; set; }
    public int? HomeHalfTimeScore { get; set; }
    public int? AwayHalfTimeScore { get; set; }
    public int? HomeFullTimeScore { get; set; }
    public int? AwayFullTimeScore { get; set; }
}
