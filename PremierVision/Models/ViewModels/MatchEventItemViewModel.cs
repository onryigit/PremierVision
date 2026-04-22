namespace PremierVision.Models.ViewModels;

public class MatchEventItemViewModel
{
    public int Minute { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public string PlayerName { get; init; } = string.Empty;
    public MatchEventType EventType { get; init; }
    public string? Description { get; init; }
}
