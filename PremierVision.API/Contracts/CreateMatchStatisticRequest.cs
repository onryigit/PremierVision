using System.ComponentModel.DataAnnotations;

namespace PremierVision.Contracts;

public class CreateMatchStatisticRequest
{
    [Required]
    public int FixtureId { get; set; }

    [Required]
    public int TeamId { get; set; }

    [Required]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Value { get; set; } = string.Empty;
}
