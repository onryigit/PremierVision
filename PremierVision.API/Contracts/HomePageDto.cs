namespace PremierVision.Contracts;

public class HomePageDto
{
    public int CurrentWeek { get; set; }
    public List<int> AvailableWeeks { get; set; } = [];
    public FixtureCardDto? FeaturedMatch { get; set; }
    public List<FixtureCardDto> WeeklyFixtures { get; set; } = [];
    public List<FixtureCardDto> LatestResults { get; set; } = [];
    public List<FixtureCardDto> UpcomingFixtures { get; set; } = [];
    public List<StandingRowDto> StandingsPreview { get; set; } = [];
}
