using Microsoft.AspNetCore.Mvc;
using PremierVision.Models;
using PremierVision.Models.ViewModels;
using PremierVision.Services;
using System.Diagnostics;

namespace PremierVision.Controllers;

public class HomeController(IPremierVisionApiClient apiClient) : Controller
{
    public async Task<IActionResult> Index(int? week, CancellationToken cancellationToken)
    {
        return View(await apiClient.GetHomePageAsync(week, cancellationToken));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
