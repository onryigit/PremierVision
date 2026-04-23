using Microsoft.AspNetCore.Mvc;
using PremierVision.Models.ViewModels;
using PremierVision.Services;

namespace PremierVision.Controllers;

public class StandingsController(IPremierVisionApiClient apiClient) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(new StandingsPageViewModel
        {
            Rows = await apiClient.GetStandingsAsync(cancellationToken)
        });
    }
}
