using Microsoft.AspNetCore.Mvc;
using PremierVision.Models.ViewModels.Admin;
using PremierVision.Services;

namespace PremierVision.Controllers;

public class AdminController(IPremierVisionApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            return View(await BuildViewModelAsync(cancellationToken));
        }
        catch (Exception exception)
        {
            return View(new AdminPanelViewModel
            {
                ErrorMessage = $"Admin paneli yuklenirken API baglantisi kurulamadi: {exception.Message}"
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportTeams(CancellationToken cancellationToken)
    {
        await apiClient.ImportTeamsAsync(cancellationToken);
        TempData["AdminMessage"] = "Takimlar import edildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportFixtures(CancellationToken cancellationToken)
    {
        await apiClient.ImportFixturesAsync(cancellationToken);
        TempData["AdminMessage"] = "Fikstur import edildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportLive(CancellationToken cancellationToken)
    {
        await apiClient.ImportLiveFixturesAsync(cancellationToken);
        TempData["AdminMessage"] = "Canli maclar guncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFixture(CreateFixtureInputModel input, CancellationToken cancellationToken)
    {
        if (input.HomeTeamId == input.AwayTeamId)
        {
            ModelState.AddModelError(nameof(input.AwayTeamId), "Ayni takim iki kez secilemez.");
        }

        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsyncSafe(cancellationToken, fixture: input));
        }

        await apiClient.AddFixtureAsync(input, cancellationToken);

        TempData["AdminMessage"] = "Mac eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEvent(CreateMatchEventInputModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsyncSafe(cancellationToken, @event: input));
        }

        await apiClient.AddEventAsync(input, cancellationToken);

        TempData["AdminMessage"] = "Mac olayi eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStatistic(CreateMatchStatisticInputModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsyncSafe(cancellationToken, statistic: input));
        }

        await apiClient.AddStatisticAsync(input, cancellationToken);

        TempData["AdminMessage"] = "Mac istatistigi eklendi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminPanelViewModel> BuildViewModelAsync(
        CancellationToken cancellationToken,
        CreateFixtureInputModel? fixture = null,
        CreateMatchEventInputModel? @event = null,
        CreateMatchStatisticInputModel? statistic = null)
    {
        var options = await apiClient.GetAdminOptionsAsync(cancellationToken);

        return new AdminPanelViewModel
        {
            Fixture = fixture ?? new CreateFixtureInputModel(),
            Event = @event ?? new CreateMatchEventInputModel(),
            Statistic = statistic ?? new CreateMatchStatisticInputModel(),
            Teams = options.Teams,
            Fixtures = options.Fixtures
        };
    }

    private async Task<AdminPanelViewModel> BuildViewModelAsyncSafe(
        CancellationToken cancellationToken,
        CreateFixtureInputModel? fixture = null,
        CreateMatchEventInputModel? @event = null,
        CreateMatchStatisticInputModel? statistic = null)
    {
        try
        {
            return await BuildViewModelAsync(cancellationToken, fixture, @event, statistic);
        }
        catch (Exception exception)
        {
            return new AdminPanelViewModel
            {
                ErrorMessage = $"API baglantisi kurulamadi: {exception.Message}",
                Fixture = fixture ?? new CreateFixtureInputModel(),
                Event = @event ?? new CreateMatchEventInputModel(),
                Statistic = statistic ?? new CreateMatchStatisticInputModel()
            };
        }
    }
}
