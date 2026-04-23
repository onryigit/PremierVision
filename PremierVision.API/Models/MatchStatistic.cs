namespace PremierVision.Models;

public class MatchStatistic
{
    public int Id { get; set; }
    public int FixtureId { get; set; }
    public int TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public Fixture Fixture { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
