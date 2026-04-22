namespace PremierVision.Models.ViewModels;

public class FixtureListViewModel
{
    public int CurrentWeek { get; init; }
    public IReadOnlyList<int> AvailableWeeks { get; init; } = [];
    public IReadOnlyList<FixtureCardViewModel> Fixtures { get; init; } = [];
}
