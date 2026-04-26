using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PremierVision.Contracts;
using PremierVision.Data;
using PremierVision.Models;
using PremierVision.Services;

namespace PremierVision.API.Controllers;

[ApiController]
[Route("api/home")]
public class HomeController(AppDbContext context, IStandingsService standingsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HomePageDto>> Get([FromQuery] int? week, CancellationToken cancellationToken)
    {
        var availableWeeks = await context.Fixtures
            .AsNoTracking()
            .Select(x => x.MatchWeek)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var currentWeek = await context.Fixtures
            .AsNoTracking()
            .Where(x => x.Status == FixtureStatus.Live || x.Status == FixtureStatus.Completed)
            .OrderByDescending(x => x.KickoffUtc)
            .Select(x => x.MatchWeek)
            .FirstOrDefaultAsync(cancellationToken);

        var selectedWeek = week
            ?? (currentWeek != 0 ? currentWeek : availableWeeks.LastOrDefault());

        var weeklyFixtures = (await context.Fixtures
            .AsNoTracking()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Include(x => x.Events)
            .Where(x => x.MatchWeek == selectedWeek)
            .OrderBy(x => x.KickoffUtc)
            .ToListAsync(cancellationToken))
            .Select(x => x.ToFixtureCardDto())
            .ToList();

        var liveFixtures = (await context.Fixtures
            .AsNoTracking()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Include(x => x.Events)
            .Where(x => x.Status == FixtureStatus.Live)
            .OrderBy(x => x.KickoffUtc)
            .Take(5)
            .ToListAsync(cancellationToken))
            .Select(x => x.ToFixtureCardDto())
            .ToList();

        var latestResults = (await context.Fixtures
            .AsNoTracking()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Where(x => x.Status == FixtureStatus.Completed)
            .OrderByDescending(x => x.KickoffUtc)
            .Take(5)
            .ToListAsync(cancellationToken))
            .Select(x => x.ToFixtureCardDto())
            .ToList();

        var upcomingFixtures = (await context.Fixtures
            .AsNoTracking()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Where(x => x.Status == FixtureStatus.NotStarted)
            .OrderBy(x => x.KickoffUtc)
            .Take(5)
            .ToListAsync(cancellationToken))
            .Select(x => x.ToFixtureCardDto())
            .ToList();

        var standings = await standingsService.CalculateAsync(cancellationToken);

        return Ok(new HomePageDto
        {
            CurrentWeek = selectedWeek,
            AvailableWeeks = availableWeeks,
            FeaturedMatch = liveFixtures
                .OrderByDescending(x => x.KickoffUtc)
                .FirstOrDefault()
                ?? latestResults.FirstOrDefault()
                ?? upcomingFixtures.FirstOrDefault()
                ?? weeklyFixtures.FirstOrDefault(),
            LiveFixtures = liveFixtures,
            WeeklyFixtures = weeklyFixtures,
            LatestResults = latestResults,
            UpcomingFixtures = upcomingFixtures,
            StandingsPreview = standings.Take(5).ToList()
        });
    }
}
