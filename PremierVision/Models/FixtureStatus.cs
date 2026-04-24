using System.ComponentModel.DataAnnotations;

namespace PremierVision.Models;

public enum FixtureStatus
{
    [Display(Name = "Başlamadı")]
    NotStarted = 0,
    [Display(Name = "Canlı")]
    Live = 1,
    [Display(Name = "Tamamlandı")]
    Completed = 2
}
