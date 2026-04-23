namespace PremierVision.Contracts;

public class MatchDetailDto
{
    public FixtureCardDto Fixture { get; set; } = new();
    public List<MatchEventItemDto> Events { get; set; } = [];
    public List<MatchStatisticItemDto> Statistics { get; set; } = [];
}
