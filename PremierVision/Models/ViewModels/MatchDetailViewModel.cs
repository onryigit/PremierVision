namespace PremierVision.Models.ViewModels;

public class MatchDetailViewModel
{
    public FixtureCardViewModel Fixture { get; init; } = new();
    public IReadOnlyList<MatchEventItemViewModel> Events { get; init; } = [];
    public IReadOnlyList<MatchStatisticItemViewModel> Statistics { get; init; } = [];
}
