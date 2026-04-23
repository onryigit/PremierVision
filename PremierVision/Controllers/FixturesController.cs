using Microsoft.AspNetCore.Mvc;
using PremierVision.Models.ViewModels;
using PremierVision.Services;

namespace PremierVision.Controllers;

public class FixturesController(IPremierVisionApiClient apiClient) : Controller
{
    public async Task<IActionResult> Index(int? week, CancellationToken cancellationToken)
    {
        return View(await apiClient.GetFixturesAsync(week, cancellationToken));
    }
}
