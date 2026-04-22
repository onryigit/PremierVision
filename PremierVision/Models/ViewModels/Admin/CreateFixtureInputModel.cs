using System.ComponentModel.DataAnnotations;

namespace PremierVision.Models.ViewModels.Admin;

public class CreateFixtureInputModel
{
    [Range(1, 100)]
    public int MatchWeek { get; set; }

    [Required]
    public int HomeTeamId { get; set; }

    [Required]
    public int AwayTeamId { get; set; }

    [Required]
    public DateTime KickoffUtc { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(150)]
    public string VenueName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public FixtureStatus Status { get; set; } = FixtureStatus.NotStarted;

    [Range(0, 50)]
    public int? HomeHalfTimeScore { get; set; }

    [Range(0, 50)]
    public int? AwayHalfTimeScore { get; set; }

    [Range(0, 50)]
    public int? HomeFullTimeScore { get; set; }

    [Range(0, 50)]
    public int? AwayFullTimeScore { get; set; }
}
