namespace PremierVision.Models;

public class Team
{
    public int Id { get; set; }
    public int? ApiFootballTeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? StadiumName { get; set; }
    public string? StadiumCity { get; set; }

    public ICollection<Player> Players { get; set; } = new List<Player>();
    public ICollection<Fixture> HomeFixtures { get; set; } = new List<Fixture>();
    public ICollection<Fixture> AwayFixtures { get; set; } = new List<Fixture>();
}
