using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PremierVision.Data;
using PremierVision.Models;
using PremierVision.Models.ViewModels;

namespace PremierVision.Controllers;

public class FixturesController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int? week, CancellationToken cancellationToken)
    {
        var availableWeeks = await context.Fixtures
            .AsNoTracking()
            .Select(x => x.MatchWeek)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var upcomingWeek = await context.Fixtures
            .AsNoTracking()
            .Where(x => x.Status != FixtureStatus.Completed)
            .OrderBy(x => x.KickoffUtc)
            .Select(x => x.MatchWeek)
            .FirstOrDefaultAsync(cancellationToken);

        var selectedWeek = week
            ?? (upcomingWeek != 0 ? upcomingWeek : availableWeeks.LastOrDefault());

        var fixtures = await context.Fixtures
            .AsNoTracking()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Where(x => x.MatchWeek == selectedWeek)
            .OrderBy(x => x.KickoffUtc)
            .Select(x => new FixtureCardViewModel
            {
                Id = x.Id,
                MatchWeek = x.MatchWeek,
                HomeTeamName = x.HomeTeam.Name,
                HomeTeamLogoUrl = x.HomeTeam.LogoUrl,
                AwayTeamName = x.AwayTeam.Name,
                AwayTeamLogoUrl = x.AwayTeam.LogoUrl,
                KickoffUtc = x.KickoffUtc,
                VenueName = x.VenueName,
                Status = x.Status,
                HomeHalfTimeScore = x.HomeHalfTimeScore,
                AwayHalfTimeScore = x.AwayHalfTimeScore,
                HomeFullTimeScore = x.HomeFullTimeScore,
                AwayFullTimeScore = x.AwayFullTimeScore
            })
            .ToListAsync(cancellationToken);

        return View(new FixtureListViewModel
        {
            CurrentWeek = selectedWeek,
            AvailableWeeks = availableWeeks,
            Fixtures = fixtures
        });
    }
}
