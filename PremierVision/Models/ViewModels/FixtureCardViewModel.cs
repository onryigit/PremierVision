namespace PremierVision.Models.ViewModels;

public class FixtureCardViewModel
{
    public int Id { get; init; }
    public int MatchWeek { get; init; }
    public string HomeTeamName { get; init; } = string.Empty;
    public string AwayTeamName { get; init; } = string.Empty;
    public DateTime KickoffUtc { get; init; }
    public string VenueName { get; init; } = string.Empty;
    public FixtureStatus Status { get; init; }
    public int? HomeHalfTimeScore { get; init; }
    public int? AwayHalfTimeScore { get; init; }
    public int? HomeFullTimeScore { get; init; }
    public int? AwayFullTimeScore { get; init; }
}
