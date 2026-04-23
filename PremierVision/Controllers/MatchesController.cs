using Microsoft.AspNetCore.Mvc;
using PremierVision.Models.ViewModels;
using PremierVision.Services;

namespace PremierVision.Controllers;

public class MatchesController(IPremierVisionApiClient apiClient) : Controller
{
    public async Task<IActionResult> Detail(int? id, CancellationToken cancellationToken)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var match = await apiClient.GetMatchDetailAsync(id.Value, cancellationToken);
        if (match is null)
        {
            return NotFound();
        }

        return View(match);
    }
}
