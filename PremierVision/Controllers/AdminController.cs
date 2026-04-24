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
                ErrorMessage = $"Admin paneli yüklenirken API bağlantısı kurulamadı: {exception.Message}"
            });
        }
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFixture([Bind(Prefix = nameof(AdminPanelViewModel.Fixture))] CreateFixtureInputModel input, CancellationToken cancellationToken)
    {
        if (input.HomeTeamId == input.AwayTeamId)
        {
            ModelState.AddModelError($"{nameof(AdminPanelViewModel.Fixture)}.{nameof(input.AwayTeamId)}", "Aynı takım iki kez seçilemez.");
        }

        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsyncSafe(cancellationToken, fixture: input));
        }

        await apiClient.AddFixtureAsync(input, cancellationToken);

        TempData["AdminMessage"] = "Maç eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEvent([Bind(Prefix = nameof(AdminPanelViewModel.Event))] CreateMatchEventInputModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsyncSafe(cancellationToken, @event: input));
        }

        await apiClient.AddEventAsync(input, cancellationToken);

        TempData["AdminMessage"] = "Maç olayı eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStatistic([Bind(Prefix = nameof(AdminPanelViewModel.Statistic))] CreateMatchStatisticInputModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsyncSafe(cancellationToken, statistic: input));
        }

        await apiClient.AddStatisticAsync(input, cancellationToken);

        TempData["AdminMessage"] = "Maç istatistiği eklendi.";
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
                ErrorMessage = $"API bağlantısı kurulamadı: {exception.Message}",
                Fixture = fixture ?? new CreateFixtureInputModel(),
                Event = @event ?? new CreateMatchEventInputModel(),
                Statistic = statistic ?? new CreateMatchStatisticInputModel()
            };
        }
    }
}
