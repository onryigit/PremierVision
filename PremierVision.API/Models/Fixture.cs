namespace PremierVision.Models;

public class Fixture
{
    public int Id { get; set; }
    public int? ApiFootballFixtureId { get; set; }
    public int MatchWeek { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public DateTime KickoffUtc { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public FixtureStatus Status { get; set; } = FixtureStatus.NotStarted;
    public int? HomeHalfTimeScore { get; set; }
    public int? AwayHalfTimeScore { get; set; }
    public int? HomeFullTimeScore { get; set; }
    public int? AwayFullTimeScore { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Team HomeTeam { get; set; } = null!;
    public Team AwayTeam { get; set; } = null!;
    public ICollection<MatchEvent> Events { get; set; } = new List<MatchEvent>();
    public ICollection<MatchStatistic> Statistics { get; set; } = new List<MatchStatistic>();
}
