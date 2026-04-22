using System.ComponentModel.DataAnnotations;

namespace PremierVision.Models.ViewModels.Admin;

public class CreateMatchEventInputModel
{
    [Required]
    public int FixtureId { get; set; }

    public int? TeamId { get; set; }

    [Range(0, 150)]
    public int Minute { get; set; }

    [Required]
    [StringLength(120)]
    public string PlayerName { get; set; } = string.Empty;

    public MatchEventType EventType { get; set; }

    [StringLength(250)]
    public string? Description { get; set; }
}
