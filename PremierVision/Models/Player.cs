namespace PremierVision.Models;

public class Player
{
    public int Id { get; set; }
    public int? ApiFootballPlayerId { get; set; }
    public int TeamId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public int? Number { get; set; }
    public string? PhotoUrl { get; set; }

    public Team Team { get; set; } = null!;
}
