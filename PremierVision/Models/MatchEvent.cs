namespace PremierVision.Models;

public class MatchEvent
{
    public int Id { get; set; }
    public int FixtureId { get; set; }
    public int? TeamId { get; set; }
    public int Minute { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public MatchEventType EventType { get; set; }
    public string? Description { get; set; }

    public Fixture Fixture { get; set; } = null!;
    public Team? Team { get; set; }
}
