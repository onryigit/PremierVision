namespace PremierVision.Models.ViewModels.Admin;

public class AdminPanelViewModel
{
    public CreateFixtureInputModel Fixture { get; init; } = new();
    public CreateMatchEventInputModel Event { get; init; } = new();
    public CreateMatchStatisticInputModel Statistic { get; init; } = new();
    public IReadOnlyList<SimpleOptionViewModel> Teams { get; init; } = [];
    public IReadOnlyList<SimpleOptionViewModel> Fixtures { get; init; } = [];
}
