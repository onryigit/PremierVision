using System.ComponentModel.DataAnnotations;

namespace PremierVision.Models;

public enum MatchEventType
{
    [Display(Name = "Gol")]
    Goal = 0,
    [Display(Name = "Sarı Kart")]
    YellowCard = 1,
    [Display(Name = "Kırmızı Kart")]
    RedCard = 2,
    [Display(Name = "Oyuncu Değişikliği")]
    Substitution = 3
}
