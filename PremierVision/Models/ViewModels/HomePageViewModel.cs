namespace PremierVision.Models.ViewModels;

public class HomePageViewModel
{
    public int CurrentWeek { get; init; }
    public IReadOnlyList<int> AvailableWeeks { get; init; } = [];
    public FixtureCardViewModel? FeaturedMatch { get; init; }
    public IReadOnlyList<FixtureCardViewModel> WeeklyFixtures { get; init; } = [];
    public IReadOnlyList<FixtureCardViewModel> LatestResults { get; init; } = [];
    public IReadOnlyList<FixtureCardViewModel> UpcomingFixtures { get; init; } = [];
    public IReadOnlyList<StandingRowViewModel> StandingsPreview { get; init; } = [];
}
