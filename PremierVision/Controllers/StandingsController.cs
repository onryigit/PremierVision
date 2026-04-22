using Microsoft.AspNetCore.Mvc;
using PremierVision.Models.ViewModels;
using PremierVision.Services;

namespace PremierVision.Controllers;

public class StandingsController(IStandingsService standingsService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var rows = await standingsService.CalculateAsync(cancellationToken);
        return View(new StandingsPageViewModel
        {
            Rows = rows
        });
    }
}
