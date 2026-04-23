namespace PremierVision.Contracts;

public class AdminPanelDto
{
    public List<SimpleOptionDto> Teams { get; set; } = [];
    public List<SimpleOptionDto> Fixtures { get; set; } = [];
}
