using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PremierVision.Data;
using PremierVision.Models;
using PremierVision.Models.ViewModels.Admin;
using PremierVision.Services;

namespace PremierVision.Controllers;

public class AdminController(AppDbContext context, IApiFootballSyncService apiFootballSyncService) : Controller
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
                ErrorMessage = $"Admin paneli yuklenirken veritabani baglantisi kurulamadi: {exception.Message}"
            });
        }
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

        context.Fixtures.Add(new Fixture
        {
            MatchWeek = input.MatchWeek,
            HomeTeamId = input.HomeTeamId,
            AwayTeamId = input.AwayTeamId,
            KickoffUtc = DateTime.SpecifyKind(input.KickoffUtc, DateTimeKind.Utc),
            VenueName = input.VenueName,
            ImageUrl = input.ImageUrl,
            Status = input.Status,
            HomeHalfTimeScore = input.HomeHalfTimeScore,
            AwayHalfTimeScore = input.AwayHalfTimeScore,
            HomeFullTimeScore = input.HomeFullTimeScore,
            AwayFullTimeScore = input.AwayFullTimeScore
        });

        await context.SaveChangesAsync(cancellationToken);
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

        context.MatchEvents.Add(new MatchEvent
        {
            FixtureId = input.FixtureId,
            TeamId = input.TeamId,
            Minute = input.Minute,
            PlayerName = input.PlayerName,
            EventType = input.EventType,
            Description = input.Description
        });

        await context.SaveChangesAsync(cancellationToken);
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

        context.MatchStatistics.Add(new MatchStatistic
        {
            FixtureId = input.FixtureId,
            TeamId = input.TeamId,
            Name = input.Name,
            Value = input.Value
        });

        await context.SaveChangesAsync(cancellationToken);
        TempData["AdminMessage"] = "Mac istatistigi eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncTeams(CancellationToken cancellationToken)
    {
        try
        {
            var count = await apiFootballSyncService.SyncTeamsAsync(cancellationToken);
            TempData["AdminMessage"] = $"{count} takim senkronize edildi.";
        }
        catch (Exception exception)
        {
            TempData["AdminMessage"] = $"Takim senkronizasyonu basarisiz: {exception.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncFixtures(CancellationToken cancellationToken)
    {
        try
        {
            var count = await apiFootballSyncService.SyncFixturesAsync(cancellationToken);
            TempData["AdminMessage"] = $"{count} fikstur senkronize edildi.";
        }
        catch (Exception exception)
        {
            TempData["AdminMessage"] = $"Fikstur senkronizasyonu basarisiz: {exception.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncMatchDetails(CancellationToken cancellationToken)
    {
        try
        {
            var fixtures = await context.Fixtures
                .AsNoTracking()
                .Where(x => x.ApiFootballFixtureId.HasValue && x.Status != FixtureStatus.NotStarted)
                .Where(x => !context.MatchEvents.Any(e => e.FixtureId == x.Id) && !context.MatchStatistics.Any(s => s.FixtureId == x.Id))
                .OrderByDescending(x => x.KickoffUtc)
                .Take(5)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var syncedCount = 0;
            foreach (var fixtureId in fixtures)
            {
                try
                {
                    if (await apiFootballSyncService.SyncFixtureDetailsAsync(fixtureId, cancellationToken))
                    {
                        syncedCount++;
                    }
                }
                catch (HttpRequestException exception) when ((int?)exception.StatusCode == 429)
                {
                    TempData["AdminMessage"] = syncedCount > 0
                        ? $"{syncedCount} mac detayi senkronize edildi. API limiti nedeniyle islem erken durduruldu."
                        : "API limiti asildi. Biraz bekleyip tekrar deneyin.";
                    return RedirectToAction(nameof(Index));
                }
            }

            TempData["AdminMessage"] = fixtures.Count == 0
                ? "Detayi eksik uygun mac bulunamadi."
                : $"{syncedCount} mac detayi senkronize edildi.";
        }
        catch (Exception exception)
        {
            TempData["AdminMessage"] = $"Mac detay senkronizasyonu basarisiz: {exception.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminPanelViewModel> BuildViewModelAsync(
        CancellationToken cancellationToken,
        CreateFixtureInputModel? fixture = null,
        CreateMatchEventInputModel? @event = null,
        CreateMatchStatisticInputModel? statistic = null)
    {
        var teams = await context.Teams
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SimpleOptionViewModel
            {
                Value = x.Id,
                Label = x.Name
            })
            .ToListAsync(cancellationToken);

        var fixtures = await context.Fixtures
            .AsNoTracking()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .OrderByDescending(x => x.KickoffUtc)
            .Select(x => new SimpleOptionViewModel
            {
                Value = x.Id,
                Label = $"Hafta {x.MatchWeek} - {x.HomeTeam.Name} vs {x.AwayTeam.Name}"
            })
            .ToListAsync(cancellationToken);

        return new AdminPanelViewModel
        {
            Fixture = fixture ?? new CreateFixtureInputModel(),
            Event = @event ?? new CreateMatchEventInputModel(),
            Statistic = statistic ?? new CreateMatchStatisticInputModel(),
            Teams = teams,
            Fixtures = fixtures
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
                ErrorMessage = $"Veritabani baglantisi kurulamadi: {exception.Message}",
                Fixture = fixture ?? new CreateFixtureInputModel(),
                Event = @event ?? new CreateMatchEventInputModel(),
                Statistic = statistic ?? new CreateMatchStatisticInputModel()
            };
        }
    }
}
