namespace PremierVision.Contracts;

public class FixtureListDto
{
    public int CurrentWeek { get; set; }
    public List<int> AvailableWeeks { get; set; } = [];
    public List<FixtureCardDto> Fixtures { get; set; } = [];
}
