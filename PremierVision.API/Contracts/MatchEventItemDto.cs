namespace PremierVision.Contracts;

public class MatchEventItemDto
{
    public int Minute { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public MatchEventType EventType { get; set; }
    public string? Description { get; set; }
}
